using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using Track;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ca.Jwsm.Railroader.Api.Host.World
{
    internal sealed class TrackTopologyApplyResult
    {
        internal TrackTopologyApplyResult(int nodeCount, int segmentCount, int spanCount)
        {
            NodeCount = nodeCount;
            SegmentCount = segmentCount;
            SpanCount = spanCount;
        }

        internal int NodeCount { get; }
        internal int SegmentCount { get; }
        internal int SpanCount { get; }
    }

    internal sealed class TrackTopologyState
    {
        private static readonly System.Reflection.FieldInfo SegmentAvailableField = AccessTools.Field(typeof(TrackSegment), "<Available>k__BackingField");
        private static readonly System.Reflection.FieldInfo SegmentGroupEnabledField = AccessTools.Field(typeof(TrackSegment), "<GroupEnabled>k__BackingField");
        private static readonly System.Reflection.MethodInfo GraphInvalidateNodeMethod = AccessTools.Method(typeof(Graph), "InvalidateNode");
        private static readonly System.Reflection.MethodInfo GraphAddSegmentMethod = AccessTools.Method(typeof(Graph), "AddSegment", new[] { typeof(TrackSegment), typeof(bool) });
        private static readonly System.Reflection.MethodInfo GraphAddSpanMethod = AccessTools.Method(typeof(Graph), "AddSpan");

        internal Dictionary<string, NodePatchState> Nodes { get; } = new Dictionary<string, NodePatchState>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, SegmentPatchState> Segments { get; } = new Dictionary<string, SegmentPatchState>(StringComparer.OrdinalIgnoreCase);
        internal Dictionary<string, SpanPatchState> Spans { get; } = new Dictionary<string, SpanPatchState>(StringComparer.OrdinalIgnoreCase);

        internal static TrackTopologyState Capture(Graph graph)
        {
            var state = new TrackTopologyState();
            foreach (var node in Object.FindObjectsOfType<TrackNode>().Where(n => n != null && !string.IsNullOrWhiteSpace(n.id)))
            {
                state.Nodes[node.id] = new NodePatchState(node);
            }

            foreach (var segment in Object.FindObjectsOfType<TrackSegment>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id) && s.a != null && s.b != null))
            {
                state.Segments[segment.id] = new SegmentPatchState(segment);
            }

            foreach (var span in Object.FindObjectsOfType<TrackSpan>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id) && s.upper.HasValue && s.lower.HasValue && s.upper.Value.segment != null && s.lower.Value.segment != null))
            {
                state.Spans[span.id] = new SpanPatchState(span);
            }

            return state;
        }

        internal void Apply(WorldLayoutDefinition definition)
        {
            MergeNodes(definition.Nodes);
            MergeSegments(definition.Segments);
            MergeSpans(definition.Spans);
        }

        internal TrackTopologyApplyResult Materialize(Graph graph, Action<string> log)
        {
            var existingNodes = Object.FindObjectsOfType<TrackNode>().Where(n => n != null && !string.IsNullOrWhiteSpace(n.id)).ToDictionary(n => n.id, StringComparer.OrdinalIgnoreCase);
            var finalNodes = new Dictionary<string, TrackNode>(StringComparer.OrdinalIgnoreCase);
            var createdNodes = 0;
            foreach (var entry in Nodes.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                var isNew = !existingNodes.TryGetValue(entry.Key, out var node);
                if (isNew)
                {
                    var gameObject = new GameObject(entry.Key);
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(graph.transform, false);
                    node = gameObject.AddComponent<TrackNode>();
                    node.id = entry.Key;
                    gameObject.SetActive(true);
                }
                else
                {
                    GraphInvalidateNodeMethod?.Invoke(graph, new object[] { node });
                }

                entry.Value.ApplyTo(node);
                if (isNew)
                {
                    graph.AddNode(node);
                    createdNodes++;
                }

                finalNodes[entry.Key] = node;
                existingNodes.Remove(entry.Key);
            }

            var existingSegments = Object.FindObjectsOfType<TrackSegment>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var finalSegments = new Dictionary<string, TrackSegment>(StringComparer.OrdinalIgnoreCase);
            var createdSegments = 0;
            foreach (var entry in Segments.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!finalNodes.TryGetValue(entry.Value.StartId, out var nodeA) || !finalNodes.TryGetValue(entry.Value.EndId, out var nodeB))
                {
                    throw new InvalidOperationException("Segment '" + entry.Key + "' could not resolve endpoints.");
                }

                var isNew = !existingSegments.TryGetValue(entry.Key, out var segment);
                if (isNew)
                {
                    var gameObject = new GameObject(entry.Key);
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(graph.transform, false);
                    segment = gameObject.AddComponent<TrackSegment>();
                    segment.id = entry.Key;
                    gameObject.SetActive(true);
                }

                segment.a = nodeA;
                segment.b = nodeB;
                entry.Value.ApplyTo(segment);
                SegmentAvailableField?.SetValue(segment, true);
                SegmentGroupEnabledField?.SetValue(segment, true);
                if (isNew)
                {
                    if (GraphAddSegmentMethod != null)
                    {
                        GraphAddSegmentMethod.Invoke(graph, new object[] { segment, true });
                    }
                    else
                    {
                        graph.AddSegment(segment);
                    }

                    createdSegments++;
                }

                finalSegments[entry.Key] = segment;
                existingSegments.Remove(entry.Key);
            }

            DestroyObsolete(existingSegments.Values);
            graph.RebuildCollections();
            var existingSpans = Object.FindObjectsOfType<TrackSpan>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var createdSpans = 0;
            foreach (var entry in Spans.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            {
                if (!entry.Value.TryCreateLocations(finalSegments, out var upper, out var lower))
                {
                    log?.Invoke("Skipping span '" + entry.Key + "' because its endpoint locations could not be resolved.");
                    continue;
                }

                var isNew = !existingSpans.TryGetValue(entry.Key, out var span);
                if (isNew)
                {
                    var gameObject = new GameObject(entry.Key);
                    gameObject.SetActive(false);
                    gameObject.transform.SetParent(graph.transform, false);
                    span = gameObject.AddComponent<TrackSpan>();
                    span.id = entry.Key;
                    gameObject.SetActive(true);
                }

                span.upper = upper.Value;
                span.lower = lower.Value;
                if (isNew)
                {
                    GraphAddSpanMethod?.Invoke(graph, new object[] { span });
                    createdSpans++;
                }

                existingSpans.Remove(entry.Key);
            }

            DestroyObsolete(existingSpans.Values);
            DestroyObsolete(existingNodes.Values);
            graph.RebuildCollections();
            return new TrackTopologyApplyResult(createdNodes, createdSegments, createdSpans);
        }

        private static void DestroyObsolete<TComponent>(IEnumerable<TComponent> components) where TComponent : Component
        {
            if (components == null)
            {
                return;
            }

            foreach (var component in components)
            {
                if (component == null)
                {
                    continue;
                }

                var gameObject = component.gameObject;
                if (gameObject == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(gameObject);
                }
                else
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        private void MergeNodes(JObject nodesObject)
        {
            if (nodesObject == null)
            {
                return;
            }

            foreach (var property in nodesObject.Properties())
            {
                if (property.Value == null || property.Value.Type == JTokenType.Null)
                {
                    Nodes.Remove(property.Name);
                    continue;
                }

                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                Nodes[property.Name] = NodePatchState.FromDefinition(definition, Nodes.ContainsKey(property.Name) ? Nodes[property.Name] : null);
            }
        }

        private void MergeSegments(JObject segmentsObject)
        {
            if (segmentsObject == null)
            {
                return;
            }

            foreach (var property in segmentsObject.Properties())
            {
                if (property.Value == null || property.Value.Type == JTokenType.Null)
                {
                    Segments.Remove(property.Name);
                    continue;
                }

                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                Segments[property.Name] = SegmentPatchState.FromDefinition(definition, Segments.ContainsKey(property.Name) ? Segments[property.Name] : null);
            }
        }

        private void MergeSpans(JObject spansObject)
        {
            if (spansObject == null)
            {
                return;
            }

            foreach (var property in spansObject.Properties())
            {
                if (property.Value == null || property.Value.Type == JTokenType.Null)
                {
                    Spans.Remove(property.Name);
                    continue;
                }

                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                Spans[property.Name] = SpanPatchState.FromDefinition(definition, Spans.ContainsKey(property.Name) ? Spans[property.Name] : null);
            }
        }
    }

    internal sealed class NodePatchState
    {
        internal NodePatchState(TrackNode node)
        {
            Position = node.transform.position;
            Rotation = node.transform.eulerAngles;
            FlipSwitchStand = node.flipSwitchStand;
        }

        private NodePatchState()
        {
        }

        internal Vector3 Position { get; private set; }
        internal Vector3 Rotation { get; private set; }
        internal bool FlipSwitchStand { get; private set; }

        internal static NodePatchState FromDefinition(JObject definition, NodePatchState seed)
        {
            var state = seed ?? new NodePatchState();
            if (definition["position"] is JObject position)
            {
                state.Position = GraphPatchParsing.ParseVector(position);
            }

            if (definition["rotation"] is JObject rotation)
            {
                state.Rotation = GraphPatchParsing.ParseVector(rotation);
            }

            state.FlipSwitchStand = definition.Value<bool?>("flipSwitchStand") ?? state.FlipSwitchStand;
            return state;
        }

        internal void ApplyTo(TrackNode node)
        {
            node.flipSwitchStand = FlipSwitchStand;
            node.transform.position = Position;
            node.transform.rotation = Quaternion.Euler(Rotation);
        }
    }

    internal sealed class SegmentPatchState
    {
        internal SegmentPatchState(TrackSegment segment)
        {
            StartId = segment.a.id;
            EndId = segment.b.id;
            GroupId = segment.groupId;
            Priority = segment.priority;
            SpeedLimit = segment.speedLimit;
            StyleName = segment.style.ToString();
            TrackClassName = segment.trackClass.ToString();
        }

        private SegmentPatchState()
        {
            GroupId = string.Empty;
            SpeedLimit = 45;
        }

        internal string StartId { get; private set; }
        internal string EndId { get; private set; }
        internal string GroupId { get; private set; }
        internal int Priority { get; private set; }
        internal int SpeedLimit { get; private set; }
        internal string StyleName { get; private set; }
        internal string TrackClassName { get; private set; }

        internal static SegmentPatchState FromDefinition(JObject definition, SegmentPatchState seed)
        {
            var state = seed ?? new SegmentPatchState();
            state.StartId = definition.Value<string>("startId") ?? state.StartId;
            state.EndId = definition.Value<string>("endId") ?? state.EndId;
            state.GroupId = definition.Value<string>("groupId") ?? state.GroupId;
            state.Priority = definition.Value<int?>("priority") ?? state.Priority;
            state.SpeedLimit = definition.Value<int?>("speedLimit") ?? state.SpeedLimit;
            state.StyleName = definition.Value<string>("style") ?? definition.Value<string>("Style") ?? state.StyleName;
            state.TrackClassName = definition.Value<string>("trackClass") ?? state.TrackClassName;
            return state;
        }

        internal void ApplyTo(TrackSegment segment)
        {
            segment.groupId = GroupId;
            segment.priority = Priority;
            segment.speedLimit = SpeedLimit;
            segment.style = GraphPatchParsing.ParseEnum(StyleName, segment.style);
            segment.trackClass = GraphPatchParsing.ParseEnum(TrackClassName, segment.trackClass);
            segment.InvalidateCurve();
        }
    }

    internal sealed class SpanPatchState
    {
        internal SpanPatchState(TrackSpan span)
        {
            var upper = span.upper.Value;
            var lower = span.lower.Value;
            UpperSegmentId = upper.segment.id;
            UpperDistance = upper.distance;
            UpperEnd = upper.end == TrackSegment.End.A ? "A" : "B";
            LowerSegmentId = lower.segment.id;
            LowerDistance = lower.distance;
            LowerEnd = lower.end == TrackSegment.End.A ? "A" : "B";
        }

        private SpanPatchState()
        {
            UpperSegmentId = string.Empty;
            UpperEnd = "B";
            LowerSegmentId = string.Empty;
            LowerEnd = "A";
        }

        internal string UpperSegmentId { get; private set; }
        internal float UpperDistance { get; private set; }
        internal string UpperEnd { get; private set; }
        internal string LowerSegmentId { get; private set; }
        internal float LowerDistance { get; private set; }
        internal string LowerEnd { get; private set; }

        internal static SpanPatchState FromDefinition(JObject definition, SpanPatchState seed)
        {
            var state = seed ?? new SpanPatchState();
            if (definition["upper"] is JObject upper)
            {
                state.UpperSegmentId = upper.Value<string>("segmentId") ?? state.UpperSegmentId;
                state.UpperDistance = upper.Value<float?>("distance") ?? state.UpperDistance;
                state.UpperEnd = upper.Value<string>("end") ?? state.UpperEnd;
            }

            if (definition["lower"] is JObject lower)
            {
                state.LowerSegmentId = lower.Value<string>("segmentId") ?? state.LowerSegmentId;
                state.LowerDistance = lower.Value<float?>("distance") ?? state.LowerDistance;
                state.LowerEnd = lower.Value<string>("end") ?? state.LowerEnd;
            }

            return state;
        }

        internal bool TryCreateLocations(Dictionary<string, TrackSegment> segments, out Location? upper, out Location? lower)
        {
            upper = null;
            lower = null;
            if (!segments.TryGetValue(UpperSegmentId, out var upperSegment) || !segments.TryGetValue(LowerSegmentId, out var lowerSegment))
            {
                return false;
            }

            upper = new Location(upperSegment, UpperDistance, ParseEnd(UpperEnd));
            lower = new Location(lowerSegment, LowerDistance, ParseEnd(LowerEnd));
            return true;
        }

        private static TrackSegment.End ParseEnd(string value)
        {
            return string.Equals(value, "start", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "a", StringComparison.OrdinalIgnoreCase)
                ? TrackSegment.End.A
                : TrackSegment.End.B;
        }
    }

    internal static class GraphPatchParsing
    {
        internal static Vector3 ParseVector(JObject value)
        {
            return value == null ? Vector3.zero : new Vector3(value.Value<float?>("x") ?? 0f, value.Value<float?>("y") ?? 0f, value.Value<float?>("z") ?? 0f);
        }

        internal static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse(value, true, out TEnum parsed) ? parsed : fallback;
        }
    }
}
