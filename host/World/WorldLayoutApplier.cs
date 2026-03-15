using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using HarmonyLib;
using Model;
using Model.Ops;
using Model.Ops.Definition;
using Newtonsoft.Json.Linq;
using Track;
using UI.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ca.Jwsm.Railroader.Api.Host.World
{
    internal static class WorldLayoutApplier
    {
        private static readonly Dictionary<string, GameObject> BuiltSplineys = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static readonly Dictionary<string, object> SplineyHandlerInstances = new Dictionary<string, object>(StringComparer.Ordinal);
        private static readonly System.Reflection.FieldInfo SegmentAvailableField = AccessTools.Field(typeof(TrackSegment), "<Available>k__BackingField");
        private static readonly System.Reflection.FieldInfo SegmentGroupEnabledField = AccessTools.Field(typeof(TrackSegment), "<GroupEnabled>k__BackingField");
        private static readonly System.Reflection.MethodInfo OpsRebuildCollectionsMethod = AccessTools.Method(typeof(OpsController), "RebuildCollections");
        private static readonly System.Reflection.MethodInfo GraphAddSpanMethod = AccessTools.Method(typeof(Graph), "AddSpan");
        private static readonly System.Reflection.FieldInfo IndustryIdentifierField = AccessTools.Field(typeof(IndustryComponent), "_identifier");
        private static readonly System.Reflection.FieldInfo RepairTrackLoadField = AccessTools.Field(typeof(RepairTrack), "repairPartsLoad");
        private static readonly System.Reflection.FieldInfo IndustryCacheField = AccessTools.Field(typeof(IndustryComponent), "_cachedIndustry");
        private static readonly System.Reflection.FieldInfo InterchangedLoaderInterchangeField = AccessTools.Field(typeof(InterchangedIndustryLoader), "_interchange");
        private static readonly System.Reflection.FieldInfo TurntableNodesField = AccessTools.Field(typeof(Turntable), "nodes");
        private static readonly System.Reflection.FieldInfo TurntableDefaultStopIndexField = AccessTools.Field(typeof(Turntable), "defaultStopIndex");
        private static readonly System.Reflection.FieldInfo TurntableBridgeGroupIdField = AccessTools.Field(typeof(Turntable), "bridgeGroupId");
        private static readonly System.Reflection.MethodInfo TurntableInitializeMethod = AccessTools.Method(typeof(Turntable), "InitializeIfNeeded");
        private static readonly Type MapManagerType = AccessTools.TypeByName("Map.Runtime.MapManager");
        private static readonly System.Reflection.PropertyInfo MapManagerInstanceProperty = MapManagerType != null ? AccessTools.Property(MapManagerType, "Instance") : null;
        private static readonly System.Reflection.MethodInfo MapManagerRebuildAllMethod = MapManagerType != null ? AccessTools.Method(MapManagerType, "RebuildAll") : null;
        private static readonly Type SceneryAssetInstanceType = AccessTools.TypeByName("SceneryAssetInstance");
        private static readonly System.Reflection.FieldInfo SceneryIdentifierField = SceneryAssetInstanceType != null ? AccessTools.Field(SceneryAssetInstanceType, "identifier") : null;
        private static readonly System.Reflection.MethodInfo SceneryReloadComponentsMethod = SceneryAssetInstanceType != null ? AccessTools.Method(SceneryAssetInstanceType, "ReloadComponents") : null;

        internal static string Apply(WorldLayoutDefinition definition, IWorldLayoutResolver resolver, Action<string> log)
        {
            if (definition == null)
            {
                return "No world patch definition was built.";
            }

            var graph = TrainController.Shared != null ? TrainController.Shared.graph : null;
            if (graph == null)
            {
                throw new InvalidOperationException("TrainController graph is not available yet.");
            }

            var ops = OpsController.Shared;
            PrepareSplineys(graph, definition, log);
            var graphState = TrackTopologyState.Capture(graph);
            graphState.Apply(definition);
            WorldNotificationBridge.PublishGraphWillChange(definition.Root, definition.ChangedKeys.Keys, log);
            var graphResult = graphState.Materialize(graph, log);
            var nodeCount = graphResult.NodeCount;
            var segmentCount = graphResult.SegmentCount;
            var spanCount = graphResult.SpanCount;
            graph.RebuildCollections();
            RebuildTrackObjects(log, "graph materialization");
            resolver?.PrepareWorldApply();
            var areaCount = ApplyAreas(ops, definition, resolver, log);
            var sceneryCount = ApplyScenery(definition.Scenery, log);
            var splineyCount = ApplySplineys(graph, definition, resolver, log);
            FinalizeTurntables(log);

            if (ops != null)
            {
                OpsRebuildCollectionsMethod?.Invoke(ops, null);
                ops.RebuildPopulations();
            }

            MapBuilder.Shared?.Rebuild();
            WorldNotificationBridge.PublishGraphDidChange(definition.Root, log);
            WorldNotificationBridge.PublishIndustriesDidChange(log);
            RebuildMapTerrain(log);
            return string.Format(CultureInfo.InvariantCulture, "Applied {0} contribution file(s): {1} node(s), {2} segment(s), {3} span(s), {4} area(s), {5} scenery object(s), {6} spliney handler(s).", definition.Sources.Count, nodeCount, segmentCount, spanCount, areaCount, sceneryCount, splineyCount);
        }

        private static void RebuildTrackObjects(Action<string> log, string reason)
        {
            var manager = TrackObjectManager.Instance;
            if (manager == null)
            {
                log?.Invoke("TrackObjectManager is unavailable during " + reason + "; skipping generated track rebuild.");
                return;
            }

            try
            {
                manager.Rebuild();
                log?.Invoke("Rebuilt generated track objects after " + reason + ".");
            }
            catch (Exception ex)
            {
                log?.Invoke("Generated track rebuild failed after " + reason + ": " + ex.Message);
            }
        }

        private static int ApplyNodes(Graph graph, JObject nodesObject, Action<string> log)
        {
            if (graph == null || nodesObject == null || !nodesObject.Properties().Any())
            {
                return 0;
            }

            var existing = Object.FindObjectsOfType<TrackNode>().Where(n => n != null && !string.IsNullOrWhiteSpace(n.id)).ToDictionary(n => n.id, StringComparer.OrdinalIgnoreCase);
            var count = 0;
            foreach (var property in nodesObject.Properties())
            {
                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                if (!existing.TryGetValue(property.Name, out var node))
                {
                    var gameObject = new GameObject(property.Name);
                    gameObject.transform.SetParent(graph.transform, true);
                    node = gameObject.AddComponent<TrackNode>();
                    node.id = property.Name;
                    graph.AddNode(node);
                    existing[property.Name] = node;
                    count++;
                }

                node.flipSwitchStand = definition.Value<bool?>("flipSwitchStand") ?? node.flipSwitchStand;
                ApplyTransform(node.transform, definition["position"] as JObject, definition["rotation"] as JObject, false);
            }

            return count;
        }

        private static int ApplySegments(Graph graph, JObject segmentsObject, Action<string> log)
        {
            if (graph == null || segmentsObject == null || !segmentsObject.Properties().Any())
            {
                return 0;
            }

            var nodes = Object.FindObjectsOfType<TrackNode>().Where(n => n != null && !string.IsNullOrWhiteSpace(n.id)).ToDictionary(n => n.id, StringComparer.OrdinalIgnoreCase);
            var existing = Object.FindObjectsOfType<TrackSegment>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var count = 0;
            foreach (var property in segmentsObject.Properties())
            {
                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                var startId = definition.Value<string>("startId");
                var endId = definition.Value<string>("endId");
                if (string.IsNullOrWhiteSpace(startId) || string.IsNullOrWhiteSpace(endId) || !nodes.TryGetValue(startId, out var nodeA) || !nodes.TryGetValue(endId, out var nodeB))
                {
                    log?.Invoke("Skipping segment '" + property.Name + "' because its endpoint nodes could not be resolved.");
                    continue;
                }

                if (!existing.TryGetValue(property.Name, out var segment))
                {
                    var gameObject = new GameObject(property.Name);
                    gameObject.transform.SetParent(graph.transform, true);
                    segment = gameObject.AddComponent<TrackSegment>();
                    segment.id = property.Name;
                    existing[property.Name] = segment;
                    graph.AddSegment(segment);
                    count++;
                }

                segment.a = nodeA;
                segment.b = nodeB;
                segment.groupId = definition.Value<string>("groupId") ?? segment.groupId;
                segment.priority = definition.Value<int?>("priority") ?? segment.priority;
                segment.speedLimit = definition.Value<int?>("speedLimit") ?? segment.speedLimit;
                segment.style = ParseEnum(definition.Value<string>("style") ?? definition.Value<string>("Style"), segment.style);
                segment.trackClass = ParseEnum(definition.Value<string>("trackClass"), segment.trackClass);
                SegmentAvailableField?.SetValue(segment, true);
                SegmentGroupEnabledField?.SetValue(segment, true);
            }

            return count;
        }

        private static int ApplySpans(Graph graph, JObject spansObject, Action<string> log)
        {
            if (graph == null || spansObject == null || !spansObject.Properties().Any())
            {
                return 0;
            }

            var segments = Object.FindObjectsOfType<TrackSegment>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var existing = Object.FindObjectsOfType<TrackSpan>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var count = 0;
            foreach (var property in spansObject.Properties())
            {
                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                if (!TryParseLocation(definition["upper"] as JObject, segments, out var upper) || !TryParseLocation(definition["lower"] as JObject, segments, out var lower))
                {
                    log?.Invoke("Skipping span '" + property.Name + "' because its endpoint locations could not be resolved.");
                    continue;
                }

                if (!existing.TryGetValue(property.Name, out var span))
                {
                    var gameObject = new GameObject(property.Name);
                    gameObject.transform.SetParent(graph.transform, true);
                    span = gameObject.AddComponent<TrackSpan>();
                    span.id = property.Name;
                    GraphAddSpanMethod?.Invoke(graph, new object[] { span });
                    existing[property.Name] = span;
                    count++;
                }

                span.upper = upper;
                span.lower = lower;
            }

            return count;
        }

        private static int ApplyAreas(OpsController ops, WorldLayoutDefinition definition, IWorldLayoutResolver resolver, Action<string> log)
        {
            var areasObject = definition != null ? definition.Areas : null;
            if (ops == null || areasObject == null || !areasObject.Properties().Any())
            {
                return 0;
            }

            var existingAreas = Object.FindObjectsOfType<Area>().Where(a => a != null && !string.IsNullOrWhiteSpace(a.identifier)).ToDictionary(a => a.identifier, StringComparer.OrdinalIgnoreCase);
            var existingSpans = Object.FindObjectsOfType<TrackSpan>().Where(s => s != null && !string.IsNullOrWhiteSpace(s.id)).ToDictionary(s => s.id, StringComparer.OrdinalIgnoreCase);
            var loads = ResolveLoads();
            var created = 0;
            var areaOrders = new List<KeyValuePair<Area, int>>();

            foreach (var areaProperty in areasObject.Properties())
            {
                var areaDefinition = areaProperty.Value as JObject;
                if (areaDefinition == null)
                {
                    continue;
                }

                existingAreas.TryGetValue(areaProperty.Name, out var baseArea);
                var syntheticAreaName = ResolveSyntheticAreaName(areaProperty.Name, areaDefinition, definition);
                var targetAreaId = string.IsNullOrWhiteSpace(syntheticAreaName)
                    ? areaProperty.Name
                    : areaProperty.Name + "__" + Slugify(syntheticAreaName);
                var targetAreaName = areaDefinition.Value<string>("name") ?? syntheticAreaName;

                if (!existingAreas.TryGetValue(targetAreaId, out var area))
                {
                    var areaObjectInstance = new GameObject(targetAreaName ?? areaProperty.Name);
                    areaObjectInstance.transform.SetParent(ops.transform, false);
                    area = areaObjectInstance.AddComponent<Area>();
                    area.identifier = targetAreaId;
                    existingAreas[area.identifier] = area;
                    created++;
                }

                area.identifier = targetAreaId;
                area.radius = areaDefinition.Value<float?>("radius") ?? (baseArea != null ? baseArea.radius : area.radius);
                area.tagColor = ParseColor(areaDefinition["tagColor"], baseArea != null ? baseArea.tagColor : area.tagColor);
                area.name = targetAreaName ?? area.name;
                var areaPosition = (areaDefinition["localPosition"] as JObject) ?? (areaDefinition["position"] as JObject) ?? ResolveSyntheticAreaPosition(syntheticAreaName, definition);
                ApplyTransform(area.transform, areaPosition, null, true);
                areaOrders.Add(new KeyValuePair<Area, int>(area, ResolveAreaOrder(ops, baseArea, syntheticAreaName, areaPosition, areaDefinition)));

                var industries = areaDefinition["industries"] as JObject;
                if (industries == null)
                {
                    continue;
                }

                foreach (var industryProperty in industries.Properties())
                {
                    var industryDefinition = industryProperty.Value as JObject;
                    if (industryDefinition == null)
                    {
                        continue;
                    }

                    var industryDisplayName = ResolveIndustryDisplayName(industryProperty.Name, industryDefinition, syntheticAreaName);
                    var industry = GetOrCreateIndustry(area.transform, industryProperty.Name, industryDisplayName);
                    industry.identifier = industryProperty.Name;
                    industry.usesContract = industryDefinition.Value<bool?>("usesContract") ?? industry.usesContract;
                    industry.name = industryDisplayName ?? industry.name;
                    ApplyTransform(industry.transform, (industryDefinition["localPosition"] as JObject) ?? (industryDefinition["position"] as JObject), null, true);

                    Interchange interchange = null;
                    var components = industryDefinition["components"] as JObject;
                    if (components == null)
                    {
                        continue;
                    }

                    foreach (var componentProperty in components.Properties())
                    {
                        var componentDefinition = componentProperty.Value as JObject;
                        if (componentDefinition == null)
                        {
                            continue;
                        }

                        var typeName = componentDefinition.Value<string>("type");
                        var component = CreateIndustryComponent(industry.transform, typeName, componentProperty.Name, componentDefinition.Value<string>("name"), componentDefinition, resolver, log, industry, loads);
                        if (component == null)
                        {
                            continue;
                        }

                        component.subIdentifier = componentProperty.Name;
                        IndustryIdentifierField?.SetValue(component, industry.identifier + "." + componentProperty.Name);
                        IndustryCacheField?.SetValue(component, industry);
                        component.sharedStorage = componentDefinition.Value<bool?>("sharedStorage") ?? component.sharedStorage;
                        component.trackSpans = ResolveTrackSpans(componentDefinition["trackSpans"] as JArray, existingSpans);
                        component.carTypeFilter = new CarTypeFilter(componentDefinition.Value<string>("carTypeFilter") ?? "*");
                        component.name = componentDefinition.Value<string>("name") ?? component.name;

                        if (component is Interchange interchangeComponent)
                        {
                            interchange = interchangeComponent;
                        }
                        else if (component is FormulaicIndustryComponent formulaicIndustry)
                        {
                            ApplyFormulaicConfiguration(formulaicIndustry, componentDefinition, loads);
                        }
                        else if (component is TeleportLoadingIndustry teleportLoadingIndustry)
                        {
                            ApplyIndustryLoaderConfiguration(teleportLoadingIndustry, componentDefinition, loads);
                            teleportLoadingIndustry.carLoadPeriod = componentDefinition.Value<float?>("carLoadPeriod") ?? teleportLoadingIndustry.carLoadPeriod;
                            teleportLoadingIndustry.carLengthFeet = componentDefinition.Value<float?>("carLengthFeet") ?? teleportLoadingIndustry.carLengthFeet;
                            teleportLoadingIndustry.inputSpans = ResolveTrackSpans(componentDefinition["inputSpans"] as JArray, existingSpans);
                            teleportLoadingIndustry.outputSpans = ResolveTrackSpans(componentDefinition["outputSpans"] as JArray, existingSpans);
                        }
                        else if (component is IndustryLoader industryLoader)
                        {
                            ApplyIndustryLoaderConfiguration(industryLoader, componentDefinition, loads);
                            industryLoader.carLoadRate = componentDefinition.Value<float?>("carTransferRate") ?? industryLoader.carLoadRate;
                            industryLoader.orderAwayLoaded = componentDefinition.Value<bool?>("orderAroundLoaded") ?? industryLoader.orderAwayLoaded;
                        }
                        else if (component is IndustryLoaderBase industryLoaderBase)
                        {
                            ApplyIndustryLoaderConfiguration(industryLoaderBase, componentDefinition, loads);
                        }
                        else if (component is InterchangedIndustryLoader interchangedLoader)
                        {
                            interchangedLoader.load = ResolveLoad(componentDefinition.Value<string>("loadId"), loads);
                            if (interchange != null)
                            {
                                InterchangedLoaderInterchangeField?.SetValue(interchangedLoader, interchange);
                            }
                        }
                        else if (component is RepairTrack repairTrack)
                        {
                            repairTrack.canOverhaul = componentDefinition.Value<bool?>("canOverhaul") ?? repairTrack.canOverhaul;
                            RepairTrackLoadField?.SetValue(repairTrack, ResolveLoad(componentDefinition.Value<string>("loadId"), loads));
                        }
                        else if (component is IndustryUnloader unloader)
                        {
                            unloader.load = ResolveLoad(componentDefinition.Value<string>("loadId"), loads);
                            unloader.storageConsumptionRate = componentDefinition.Value<float?>("storageChangeRate") ?? unloader.storageConsumptionRate;
                            unloader.maxStorage = componentDefinition.Value<float?>("maxStorage") ?? unloader.maxStorage;
                            unloader.orderAwayEmpties = componentDefinition.Value<bool?>("orderAroundEmpties") ?? unloader.orderAwayEmpties;
                            unloader.orderLoads = componentDefinition.Value<bool?>("orderAroundLoaded") ?? unloader.orderLoads;
                            unloader.carUnloadRate = componentDefinition.Value<float?>("carTransferRate") ?? unloader.carUnloadRate;
                        }
                    }

                    if (!industry.gameObject.activeSelf)
                    {
                        industry.gameObject.SetActive(true);
                    }
                }
            }

            var allAreas = Object.FindObjectsOfType<Area>()
                .Where(area => area != null && area.transform.parent == ops.transform && !string.IsNullOrWhiteSpace(area.identifier))
                .Distinct()
                .ToList();
            var desiredOrders = allAreas.ToDictionary(area => area, area => area.transform.GetSiblingIndex() * 100);
            foreach (var pair in areaOrders)
            {
                desiredOrders[pair.Key] = pair.Value;
            }

            var sortedAreas = allAreas
                .OrderBy(area => desiredOrders.TryGetValue(area, out var order) ? order : area.transform.GetSiblingIndex() * 100)
                .ThenBy(area => area.transform.GetSiblingIndex())
                .ThenBy(area => area.name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            for (var areaIndex = 0; areaIndex < sortedAreas.Count; areaIndex++)
            {
                sortedAreas[areaIndex].transform.SetSiblingIndex(Mathf.Clamp(areaIndex, 0, ops.transform.childCount - 1));
            }

            return created;
        }

        private static int ApplyScenery(JObject sceneryObject, Action<string> log)
        {
            if (sceneryObject == null || !sceneryObject.Properties().Any() || SceneryAssetInstanceType == null)
            {
                return 0;
            }

            var world = GameObject.Find("World");
            var worldRoot = GetOrCreateRoot("[MapModLoader Scenery]", world != null ? world.transform : null);
            var count = 0;
            foreach (var property in sceneryObject.Properties())
            {
                var definition = property.Value as JObject;
                if (definition == null)
                {
                    continue;
                }

                var existing = worldRoot.Find(property.Name);
                var gameObject = existing != null ? existing.gameObject : new GameObject(property.Name);
                var isNew = existing == null;
                if (isNew)
                {
                    gameObject.SetActive(false);
                }

                gameObject.transform.SetParent(worldRoot, false);
                ApplyTransform(gameObject.transform, definition["position"] as JObject, definition["rotation"] as JObject, false);
                ApplyScale(gameObject.transform, definition["scale"] as JObject);
                var component = gameObject.GetComponent(SceneryAssetInstanceType) ?? gameObject.AddComponent(SceneryAssetInstanceType);
                var identifier = definition.Value<string>("modelIdentifier") ?? property.Name;
                SceneryIdentifierField?.SetValue(component, identifier);
                try
                {
                    if (isNew)
                    {
                        gameObject.SetActive(true);
                    }

                    SceneryReloadComponentsMethod?.Invoke(component, null);
                }
                catch (Exception ex)
                {
                    gameObject.SetActive(false);
                    log?.Invoke("Scenery '" + property.Name + "' failed to load model '" + identifier + "': " + ex.Message);
                }

                count++;
            }

            return count;
        }

        private static void PrepareSplineys(Graph graph, WorldLayoutDefinition definition, Action<string> log)
        {
            var splineysObject = definition != null ? definition.Splineys : null;
            if (graph == null || splineysObject == null || !splineysObject.Properties().Any())
            {
                return;
            }

            foreach (var property in splineysObject.Properties())
            {
                var splineyDefinition = property.Value as JObject;
                if (splineyDefinition == null)
                {
                    continue;
                }

                var handler = splineyDefinition.Value<string>("handler") ?? string.Empty;
                if (handler.EndsWith("TurntableBuilder", StringComparison.OrdinalIgnoreCase))
                {
                    PrepareTurntableCompatibility(graph.transform, graph, property.Name, splineyDefinition, definition, log);
                }
            }
        }

        private static int ApplySplineys(Graph graph, WorldLayoutDefinition definition, IWorldLayoutResolver resolver, Action<string> log)
        {
            var splineysObject = definition != null ? definition.Splineys : null;
            if (graph == null || splineysObject == null || !splineysObject.Properties().Any())
            {
                return 0;
            }

            var runtimeRoot = graph.transform;
            var count = 0;
            foreach (var property in splineysObject.Properties())
            {
                var splineyDefinition = property.Value as JObject;
                if (splineyDefinition == null)
                {
                    continue;
                }

                var handler = splineyDefinition.Value<string>("handler") ?? string.Empty;
                if (handler.EndsWith("TurntableBuilder", StringComparison.OrdinalIgnoreCase))
                {
                    NormalizeTurntableDefinition(graph, property.Name, splineyDefinition);
                    if (TryInvokeSplineyHandler(runtimeRoot, property.Name, handler, splineyDefinition, log, out _))
                    {
                        count++;
                        continue;
                    }

                    if (ApplyNativeTurntable(runtimeRoot, graph, property.Name, splineyDefinition, log))
                    {
                        count++;
                        continue;
                    }
                }

                if (!handler.EndsWith("TurntableBuilder", StringComparison.OrdinalIgnoreCase)
                    && TryInvokeSplineyHandler(runtimeRoot, property.Name, handler, splineyDefinition, log, out _))
                {
                    count++;
                    continue;
                }

                var fallbackKind = handler.EndsWith("TurntableBuilder", StringComparison.OrdinalIgnoreCase)
                    ? "Turntable"
                    : handler.EndsWith("AutoTrestleBuilder", StringComparison.OrdinalIgnoreCase)
                        ? "AutoTrestle"
                        : handler.EndsWith("LoaderBuilder", StringComparison.OrdinalIgnoreCase)
                            ? "Loader"
                            : "Unsupported";
                ApplyPlaceholder(runtimeRoot, property.Name, splineyDefinition, fallbackKind, log);
                count++;
            }

            return count;
        }

        private static bool ApplyNativeTurntable(Transform parent, Graph graph, string id, JObject splineyDefinition, Action<string> log)
        {
            if (parent == null || graph == null || string.IsNullOrWhiteSpace(id) || splineyDefinition == null)
            {
                return false;
            }

            var child = parent.Find(id);
            var gameObject = child != null ? child.gameObject : new GameObject(id);
            var wasActive = gameObject.activeSelf;
            if (wasActive)
            {
                gameObject.SetActive(false);
            }

            gameObject.transform.SetParent(parent, false);
            ApplyTransform(gameObject.transform, splineyDefinition["position"] as JObject, splineyDefinition["rotation"] as JObject, false);

            var turntable = gameObject.GetComponent<Turntable>() ?? gameObject.AddComponent<Turntable>();
            turntable.id = id;
            turntable.radius = Mathf.Clamp(splineyDefinition.Value<float?>("radius") ?? turntable.radius, 10f, 20f);

            var subdivisions = Mathf.Max(6, splineyDefinition.Value<int?>("subdivisions") ?? turntable.subdivisions);
            if ((subdivisions & 1) == 1)
            {
                subdivisions--;
            }

            turntable.subdivisions = subdivisions;
            TurntableBridgeGroupIdField?.SetValue(turntable, splineyDefinition.Value<string>("groupId") ?? string.Empty);

            var nodes = Enumerable.Range(0, subdivisions)
                .Select(index => Object.FindObjectsOfType<TrackNode>().FirstOrDefault(node => node != null && string.Equals(node.id, "N" + id + "TurntableNode" + index.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (nodes.Any(node => node == null))
            {
                log?.Invoke("Native turntable '" + id + "' could not resolve its generated node ring.");
                return false;
            }

            foreach (var node in nodes)
            {
                node.turntable = turntable;
            }

            TurntableNodesField?.SetValue(turntable, nodes);
            var defaultStopIndex = ResolveDefaultTurntableStopIndex(graph, nodes);
            TurntableDefaultStopIndexField?.SetValue(turntable, defaultStopIndex);
            var marker = gameObject.GetComponent<NativeTurntableMarker>() ?? gameObject.AddComponent<NativeTurntableMarker>();

            if (wasActive || !gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            TurntableInitializeMethod?.Invoke(turntable, null);
            turntable.SetStopIndex(defaultStopIndex);
            log?.Invoke("Built native compatibility turntable '" + id + "' with " + subdivisions.ToString(CultureInfo.InvariantCulture) + " stops.");
            return true;
        }

        private static int ResolveDefaultTurntableStopIndex(Graph graph, IList<TrackNode> nodes)
        {
            if (graph == null || nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            for (var index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                if (node == null)
                {
                    continue;
                }

                if (graph.SegmentsConnectedTo(node).Any())
                {
                    return index;
                }
            }

            return 0;
        }

        private static void NormalizeTurntableDefinition(Graph graph, string id, JObject splineyDefinition)
        {
            if (graph == null || splineyDefinition == null || string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var nodes = Object.FindObjectsOfType<TrackNode>()
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.id) && node.id.StartsWith("N" + id + "TurntableNode", StringComparison.OrdinalIgnoreCase))
                .OrderBy(node => ParseTrailingInteger(node.id))
                .ToList();
            if (nodes.Count < 2)
            {
                return;
            }

            var center = Vector3.zero;
            for (var index = 0; index < nodes.Count; index++)
            {
                center += nodes[index].transform.position;
            }

            center /= nodes.Count;
            var forward = (nodes[0].transform.position - center).normalized;
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            var radius = nodes.Average(node => Vector3.Distance(center, node.transform.position));
            splineyDefinition["position"] = CreateVectorToken(center);
            splineyDefinition["rotation"] = CreateVectorToken(new Vector3(0f, Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y, 0f));
            splineyDefinition["radius"] = radius;
            splineyDefinition["subdivisions"] = nodes.Count;
        }

        private static void PrepareTurntableCompatibility(Transform parent, Graph graph, string id, JObject splineyDefinition, WorldLayoutDefinition definition, Action<string> log)
        {
            if (graph == null || string.IsNullOrWhiteSpace(id) || splineyDefinition == null)
            {
                return;
            }

            var center = splineyDefinition["position"] is JObject position ? GraphPatchParsing.ParseVector(position) : Vector3.zero;
            var rotation = splineyDefinition["rotation"] as JObject;
            var yaw = rotation != null ? GraphPatchParsing.ParseVector(rotation).y : 0f;
            var radius = splineyDefinition.Value<float?>("radius") ?? 15f;
            var subdivisions = Mathf.Max(6, splineyDefinition.Value<int?>("subdivisions") ?? 32);
            if ((subdivisions & 1) == 1)
            {
                subdivisions--;
            }
            EnsureTurntableNodes(graph, definition, id, center, yaw, radius, subdivisions, log);
        }

        private static void EnsureTurntableNodes(Graph graph, WorldLayoutDefinition definition, string turntableId, Vector3 center, float yaw, float radius, int subdivisions, Action<string> log)
        {
            if (graph == null || definition == null || string.IsNullOrWhiteSpace(turntableId))
            {
                return;
            }

            var nodePrefix = "N" + turntableId + "TurntableNode";
            var existingNodes = Object.FindObjectsOfType<TrackNode>()
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.id))
                .ToDictionary(node => node.id, StringComparer.OrdinalIgnoreCase);

            for (var slot = 0; slot < subdivisions; slot++)
            {
                var generatedNodeId = nodePrefix + slot.ToString(CultureInfo.InvariantCulture);
                var angle = yaw + (slot * (360f / Mathf.Max(1, subdivisions)));
                var rotation = Quaternion.Euler(0f, angle, 0f);
                var position = center + (rotation * Vector3.forward * radius);

                if (existingNodes.TryGetValue(generatedNodeId, out var existingNode))
                {
                    existingNode.transform.position = position;
                    existingNode.transform.rotation = rotation;
                    existingNode.flipSwitchStand = false;
                    continue;
                }

                var nodeObject = new GameObject(generatedNodeId);
                nodeObject.transform.SetParent(graph.transform, false);
                nodeObject.transform.position = position;
                nodeObject.transform.rotation = rotation;
                var node = nodeObject.AddComponent<TrackNode>();
                node.id = generatedNodeId;
                node.flipSwitchStand = false;
                graph.AddNode(node);
                existingNodes[generatedNodeId] = node;
                log?.Invoke("Created compatibility turntable node '" + generatedNodeId + "'.");
            }
        }

        private static int ParseTrailingInteger(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return -1;
            }

            var digits = new string(value.Reverse().TakeWhile(char.IsDigit).Reverse().ToArray());
            int result;
            return int.TryParse(digits, out result) ? result : -1;
        }

        private static void FinalizeTurntables(Action<string> log)
        {
            foreach (var marker in Object.FindObjectsOfType<NativeTurntableMarker>())
            {
                var turntable = marker != null ? marker.GetComponent<Turntable>() : null;
                if (turntable == null || string.IsNullOrWhiteSpace(turntable.id))
                {
                    continue;
                }

                var nodes = Object.FindObjectsOfType<TrackNode>()
                    .Where(node => node != null && !string.IsNullOrWhiteSpace(node.id) && node.id.StartsWith("N" + turntable.id + "TurntableNode", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(node => ParseTrailingInteger(node.id))
                    .ToList();
                if (nodes.Count < 2)
                {
                    continue;
                }

                var center = Vector3.zero;
                for (var index = 0; index < nodes.Count; index++)
                {
                    center += nodes[index].transform.position;
                }

                center /= nodes.Count;
                var forward = (nodes[0].transform.position - center).normalized;
                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }

                turntable.transform.position = center;
                turntable.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                TurntableInitializeMethod?.Invoke(turntable, null);
                var defaultStopIndex = TurntableDefaultStopIndexField != null
                    ? (int)TurntableDefaultStopIndexField.GetValue(turntable)
                    : ResolveDefaultTurntableStopIndex(TrainController.Shared != null ? TrainController.Shared.graph : null, nodes);
                turntable.SetStopIndex(defaultStopIndex);
                log?.Invoke("Finalized turntable '" + turntable.id + "' after spliney rebuild.");
            }
        }

        private static bool TryInvokeSplineyHandler(Transform parent, string id, string handlerName, JObject definition, Action<string> log, out GameObject builtObject)
        {
            builtObject = null;
            if (parent == null || string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(handlerName) || definition == null)
            {
                return false;
            }

            var handlerType = ResolveHandlerType(handlerName);
            if (handlerType == null)
            {
                log?.Invoke("Unsupported spliney handler '" + handlerName + "' for '" + id + "'.");
                return false;
            }

            if (!TryCreateHandler(handlerType, out var handler))
            {
                log?.Invoke("Could not create spliney handler '" + handlerName + "' for '" + id + "'.");
                return false;
            }

            var buildMethod = handlerType.GetMethod("BuildSpliney", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(Transform), typeof(JObject) }, null)
                ?? handlerType.GetMethod("BuildSplineyInternal", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(Transform), typeof(JObject) }, null);
            if (buildMethod == null)
            {
                log?.Invoke("Spliney handler '" + handlerName + "' does not expose a supported build method.");
                return false;
            }

            try
            {
                if (BuiltSplineys.TryGetValue(id, out var existingSpliney))
                {
                    if (existingSpliney != null)
                    {
                        Object.Destroy(existingSpliney);
                    }

                    BuiltSplineys.Remove(id);
                }

                var result = buildMethod.Invoke(handler, new object[] { id, parent, definition });
                builtObject = result as GameObject;
                if (builtObject != null)
                {
                    BuiltSplineys[id] = builtObject;
                }

                return true;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                log?.Invoke("Spliney handler '" + handlerName + "' failed for '" + id + "': " + ex.InnerException.Message);
                return false;
            }
            catch (Exception ex)
            {
                log?.Invoke("Spliney handler '" + handlerName + "' failed for '" + id + "': " + ex.Message);
                return false;
            }
        }


        private static Type ResolveHandlerType(string handlerName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(handlerName, false);
                if (type != null)
                {
                    return type;
                }
            }

            if (string.Equals(handlerName, "AlinasMapMod.LoaderBuilder", StringComparison.OrdinalIgnoreCase))
            {
                return ResolveHandlerType("AlinasMapMod.Loaders.LoaderBuilder");
            }

            var simpleName = handlerName.Split('.').LastOrDefault();
            if (!string.IsNullOrWhiteSpace(simpleName))
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type[] types;
                    try
                    {
                        types = assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        types = ex.Types.Where(type => type != null).ToArray();
                    }

                    var suffixMatch = types.FirstOrDefault(type => type != null && type.FullName != null && (type.FullName.EndsWith("." + simpleName, StringComparison.OrdinalIgnoreCase) || type.FullName.EndsWith("+" + simpleName, StringComparison.OrdinalIgnoreCase)));
                    if (suffixMatch != null)
                    {
                        return suffixMatch;
                    }
                }
            }

            return null;
        }

        private static bool TryCreateHandler(Type handlerType, out object handler)
        {
            handler = null;
            if (handlerType == null)
            {
                return false;
            }

            var cacheKey = handlerType.FullName ?? handlerType.Name;
            if (SplineyHandlerInstances.TryGetValue(cacheKey, out handler) && handler != null)
            {
                return true;
            }

            try
            {
                handler = Activator.CreateInstance(handlerType);
                if (handler != null)
                {
                    SplineyHandlerInstances[cacheKey] = handler;
                }

                return true;
            }
            catch
            {
                handler = null;
                return false;
            }
        }

        private static void ApplyPlaceholder(Transform parent, string id, JObject definition, string kind, Action<string> log)
        {
            if (parent.Find(id) != null)
            {
                return;
            }

            var gameObject = new GameObject(id + " [" + kind + "]");
            gameObject.transform.SetParent(parent, false);
            ApplyTransform(gameObject.transform, definition["position"] as JObject, definition["rotation"] as JObject, false);
            log?.Invoke("Created compatibility placeholder for '" + id + "' using handler kind '" + kind + "'.");
        }

        private static int ResolveAreaOrder(OpsController ops, Area baseArea, string syntheticAreaName, JObject areaPosition, JObject areaDefinition)
        {
            var explicitOrder = areaDefinition.Value<int?>("order");
            if (explicitOrder.HasValue)
            {
                return explicitOrder.Value;
            }

            if (string.IsNullOrWhiteSpace(syntheticAreaName) || ops == null || areaPosition == null)
            {
                return baseArea != null ? baseArea.transform.GetSiblingIndex() * 100 : 0;
            }

            var syntheticPosition = ParseVector(areaPosition);
            var orderedAreas = Object.FindObjectsOfType<Area>()
                .Where(area => area != null && area.transform.parent == ops.transform && !string.IsNullOrWhiteSpace(area.identifier))
                .OrderBy(area => area.transform.GetSiblingIndex())
                .ToList();
            if (orderedAreas.Count == 0)
            {
                return 0;
            }

            if (orderedAreas.Count == 1)
            {
                return orderedAreas[0].transform.GetSiblingIndex() * 100 + 50;
            }

            if (baseArea != null)
            {
                var baseIndex = orderedAreas.IndexOf(baseArea);
                if (baseIndex >= 0)
                {
                    var baseOrder = baseArea.transform.GetSiblingIndex() * 100;
                    var previousArea = baseIndex > 0 ? orderedAreas[baseIndex - 1] : null;
                    var nextArea = baseIndex < orderedAreas.Count - 1 ? orderedAreas[baseIndex + 1] : null;
                    if (previousArea != null && nextArea != null)
                    {
                        var previousDistance = DistanceToSegmentXZ(syntheticPosition, previousArea.transform.position, baseArea.transform.position);
                        var nextDistance = DistanceToSegmentXZ(syntheticPosition, baseArea.transform.position, nextArea.transform.position);
                        return previousDistance <= nextDistance
                            ? (previousArea.transform.GetSiblingIndex() * 100 + baseOrder) / 2
                            : (baseOrder + nextArea.transform.GetSiblingIndex() * 100) / 2;
                    }

                    if (previousArea != null)
                    {
                        return (previousArea.transform.GetSiblingIndex() * 100 + baseOrder) / 2;
                    }

                    if (nextArea != null)
                    {
                        return (baseOrder + nextArea.transform.GetSiblingIndex() * 100) / 2;
                    }
                }
            }

            var bestIndex = 0;
            var bestDistance = float.MaxValue;
            for (var index = 0; index < orderedAreas.Count - 1; index++)
            {
                var distance = DistanceToSegmentXZ(syntheticPosition, orderedAreas[index].transform.position, orderedAreas[index + 1].transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return (orderedAreas[bestIndex].transform.GetSiblingIndex() * 100 + orderedAreas[bestIndex + 1].transform.GetSiblingIndex() * 100) / 2;
        }

        private static float DistanceToSegmentXZ(Vector3 point, Vector3 start, Vector3 end)
        {
            var point2 = new Vector2(point.x, point.z);
            var start2 = new Vector2(start.x, start.z);
            var end2 = new Vector2(end.x, end.z);
            var segment = end2 - start2;
            var lengthSquared = segment.sqrMagnitude;
            if (lengthSquared < 0.0001f)
            {
                return Vector2.Distance(point2, start2);
            }

            var t = Mathf.Clamp01(Vector2.Dot(point2 - start2, segment) / lengthSquared);
            var projection = start2 + segment * t;
            return Vector2.Distance(point2, projection);
        }

        private static string ResolveSyntheticAreaName(string baseAreaId, JObject areaDefinition, WorldLayoutDefinition definition)
        {
            if (areaDefinition == null || definition == null || !string.IsNullOrWhiteSpace(areaDefinition.Value<string>("name")))
            {
                return null;
            }

            var industries = areaDefinition["industries"] as JObject;
            var dominantPrefix = ResolveDominantIndustryPrefix(industries);
            if (string.IsNullOrWhiteSpace(dominantPrefix) || string.Equals(dominantPrefix, baseAreaId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            foreach (var property in definition.Splineys.Properties())
            {
                var spliney = property.Value as JObject;
                if (spliney == null)
                {
                    continue;
                }

                var handler = spliney.Value<string>("handler") ?? string.Empty;
                var text = spliney.Value<string>("text");
                if (handler.EndsWith("MapLabelBuilder", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(text) && text.IndexOf(dominantPrefix, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return text;
                }
            }

            var firstIndustryName = industries?.Properties()
                .Select(property => (property.Value as JObject)?.Value<string>("name"))
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name) && name.IndexOf(dominantPrefix, StringComparison.OrdinalIgnoreCase) >= 0);
            return firstIndustryName;
        }

        private static string ResolveIndustryDisplayName(string industryIdentifier, JObject industryDefinition, string syntheticAreaName)
        {
            var explicitName = industryDefinition.Value<string>("name");
            if (!string.IsNullOrWhiteSpace(explicitName))
            {
                return explicitName;
            }

            var components = industryDefinition["components"] as JObject;
            if (components != null)
            {
                var componentName = components.Properties()
                    .Select(property => (property.Value as JObject)?.Value<string>("name"))
                    .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name));
                if (!string.IsNullOrWhiteSpace(componentName))
                {
                    return componentName;
                }
            }

            var prefix = ExtractIndustryPrefix(industryIdentifier);
            if (!string.IsNullOrWhiteSpace(syntheticAreaName) &&
                !string.IsNullOrWhiteSpace(prefix) &&
                syntheticAreaName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return syntheticAreaName;
            }

            return HumanizeIdentifier(industryIdentifier);
        }

        private static JObject ResolveSyntheticAreaPosition(string syntheticAreaName, WorldLayoutDefinition definition)
        {
            if (string.IsNullOrWhiteSpace(syntheticAreaName) || definition == null)
            {
                return null;
            }

            foreach (var property in definition.Splineys.Properties())
            {
                var spliney = property.Value as JObject;
                if (spliney == null)
                {
                    continue;
                }

                var handler = spliney.Value<string>("handler") ?? string.Empty;
                var text = spliney.Value<string>("text");
                if (!handler.EndsWith("MapLabelBuilder", StringComparison.OrdinalIgnoreCase) || !string.Equals(text, syntheticAreaName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return spliney["position"] as JObject;
            }

            return null;
        }

        private static string ResolveDominantIndustryPrefix(JObject industries)
        {
            if (industries == null)
            {
                return null;
            }

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var property in industries.Properties())
            {
                var prefix = ExtractIndustryPrefix(property.Name);
                if (string.IsNullOrWhiteSpace(prefix))
                {
                    continue;
                }

                counts[prefix] = counts.TryGetValue(prefix, out var current) ? current + 1 : 1;
            }

            return counts.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase).Select(pair => pair.Key).FirstOrDefault();
        }

        private static string ExtractIndustryPrefix(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            var normalized = identifier;
            if (normalized.StartsWith("CF_", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(3);
            }

            var separatorIndex = normalized.IndexOfAny(new[] { '-', '_' });
            if (separatorIndex > 0)
            {
                normalized = normalized.Substring(0, separatorIndex);
            }
            else
            {
                normalized = new string(normalized.TakeWhile((character, index) => index == 0 || !char.IsUpper(character)).ToArray());
            }

            return normalized.Trim();
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var characters = value.ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) ? character : '-')
                .ToArray();
            return new string(characters).Trim('-');
        }

        private static string HumanizeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var normalized = value.Replace('-', ' ').Replace('_', ' ').Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return value;
            }

            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(normalized);
        }

        private static Industry GetOrCreateIndustry(Transform parent, string objectName, string displayName)
        {
            var child = parent.Find(objectName);
            if (child != null)
            {
                child.gameObject.name = displayName ?? objectName;
                var existingIndustry = child.GetComponent<Industry>() ?? child.gameObject.AddComponent<Industry>();
                existingIndustry.identifier = objectName;
                existingIndustry.name = displayName ?? objectName;
                return existingIndustry;
            }

            var gameObject = new GameObject(displayName ?? objectName);
            gameObject.name = displayName ?? objectName;
            gameObject.SetActive(false);
            gameObject.transform.SetParent(parent, false);
            var industry = gameObject.AddComponent<Industry>();
            industry.identifier = objectName;
            industry.name = displayName ?? objectName;
            return industry;
        }

        private static TComponent GetOrCreateChildComponent<TComponent>(Transform parent, string objectName, string displayName, bool createInactive) where TComponent : Component
        {
            var child = parent.Find(objectName);
            var gameObject = child != null ? child.gameObject : new GameObject(displayName ?? objectName);
            gameObject.name = displayName ?? objectName;
            if (child == null && createInactive)
            {
                gameObject.SetActive(false);
            }

            gameObject.transform.SetParent(parent, false);
            var component = gameObject.GetComponent<TComponent>() ?? gameObject.AddComponent<TComponent>();
            return component;
        }

        private static IndustryComponent CreateIndustryComponent(Transform parent, string typeName, string objectName, string displayName, JObject componentDefinition, IWorldLayoutResolver resolver, Action<string> log, Industry industry, Dictionary<string, Load> loads)
        {
            var type = ResolveIndustryComponentType(typeName, resolver);
            if (type == null || !typeof(IndustryComponent).IsAssignableFrom(type))
            {
                log?.Invoke("Skipping unsupported industry component type '" + typeName + "'.");
                return null;
            }

            var child = parent.Find(objectName);
            var gameObject = child != null ? child.gameObject : new GameObject(displayName ?? objectName);
            gameObject.name = displayName ?? objectName;
            if (child == null)
            {
                gameObject.SetActive(false);
            }

            gameObject.transform.SetParent(parent, false);
            var component = (IndustryComponent)(gameObject.GetComponent(type) ?? gameObject.AddComponent(type));
            component.subIdentifier = objectName;
            IndustryIdentifierField?.SetValue(component, industry.identifier + "." + objectName);
            IndustryCacheField?.SetValue(component, industry);
            ApplyCustomIndustryComponentData(component, componentDefinition, loads, log);
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            return component;
        }

        private static Type ResolveIndustryComponentType(string typeName, IWorldLayoutResolver resolver)
        {
            var type = resolver != null ? resolver.ResolveType(typeName) : null;
            if (type != null)
            {
                return type;
            }

            type = AccessTools.TypeByName(typeName);
            if (type != null)
            {
                return type;
            }

            if (resolver != null && resolver.TryResolveComponentKind(typeName, out var registeredType))
            {
                return registeredType;
            }

            return null;
        }

        private static void ApplyIndustryLoaderConfiguration(IndustryLoaderBase loaderBase, JObject componentDefinition, Dictionary<string, Load> loads)
        {
            if (loaderBase == null || componentDefinition == null)
            {
                return;
            }

            loaderBase.load = ResolveLoad(componentDefinition.Value<string>("loadId"), loads);
            loaderBase.productionRate = componentDefinition.Value<float?>("storageChangeRate") ?? loaderBase.productionRate;
            loaderBase.maxStorage = componentDefinition.Value<float?>("maxStorage") ?? loaderBase.maxStorage;
            loaderBase.orderEmpties = componentDefinition.Value<bool?>("orderAroundEmpties") ?? loaderBase.orderEmpties;
        }

        private static void ApplyFormulaicConfiguration(FormulaicIndustryComponent formulaicIndustry, JObject componentDefinition, Dictionary<string, Load> loads)
        {
            if (formulaicIndustry == null || componentDefinition == null)
            {
                return;
            }

            formulaicIndustry.inputTerms = BuildFormulaicTerms(componentDefinition["inputTermsPerDay"] as JObject, loads);
            formulaicIndustry.outputTerms = BuildFormulaicTerms(componentDefinition["outputTermsPerDay"] as JObject, loads);
        }

        private static List<FormulaicIndustryComponent.Term> BuildFormulaicTerms(JObject termsDefinition, Dictionary<string, Load> loads)
        {
            var terms = new List<FormulaicIndustryComponent.Term>();
            if (termsDefinition == null)
            {
                return terms;
            }

            foreach (var property in termsDefinition.Properties())
            {
                var load = ResolveLoad(property.Name, loads);
                if (load == null)
                {
                    continue;
                }

                terms.Add(new FormulaicIndustryComponent.Term
                {
                    load = load,
                    unitsPerDay = property.Value.Value<float?>() ?? 0f
                });
            }

            return terms;
        }

        private static void ApplyCustomIndustryComponentData(IndustryComponent component, JObject componentDefinition, Dictionary<string, Load> loads, Action<string> log)
        {
            if (component == null || componentDefinition == null)
            {
                return;
            }

            var customIndustryComponentType = ResolveRuntimeType("StrangeCustoms.Tracks.Industries.ICustomIndustryComponent");
            if (customIndustryComponentType == null || !customIndustryComponentType.IsAssignableFrom(component.GetType()))
            {
                return;
            }

            try
            {
                var serializedComponent = BuildSerializedComponent(componentDefinition);
                var patchingContext = BuildPatchingContext(loads);
                if (serializedComponent == null || patchingContext == null)
                {
                    return;
                }

                var deserializeMethod = customIndustryComponentType.GetMethod("DeserializeComponent", BindingFlags.Public | BindingFlags.Instance);
                deserializeMethod?.Invoke(component, new[] { serializedComponent, patchingContext });
            }
            catch (Exception ex)
            {
                log?.Invoke("Custom industry component '" + component.GetType().FullName + "' failed to deserialize: " + ex.Message);
            }
        }

        private static object BuildSerializedComponent(JObject componentDefinition)
        {
            var serializedComponentType = ResolveRuntimeType("StrangeCustoms.Tracks.SerializedComponent");
            if (serializedComponentType == null)
            {
                return null;
            }

            var serializedComponent = Activator.CreateInstance(serializedComponentType);
            if (serializedComponent == null)
            {
                return null;
            }

            SetWritableMember(serializedComponent, "Type", componentDefinition.Value<string>("type"));
            SetWritableMember(serializedComponent, "Name", componentDefinition.Value<string>("name"));
            SetWritableMember(serializedComponent, "TrackSpans", componentDefinition["trackSpans"] is JArray trackSpans ? trackSpans.Values<string>().Where(value => !string.IsNullOrWhiteSpace(value)).ToArray() : Array.Empty<string>());
            SetWritableMember(serializedComponent, "CarTypeFilter", componentDefinition.Value<string>("carTypeFilter"));
            SetWritableMember(serializedComponent, "SharedStorage", componentDefinition.Value<bool?>("sharedStorage") ?? true);
            SetWritableMember(serializedComponent, "LoadId", componentDefinition.Value<string>("loadId"));
            SetWritableMember(serializedComponent, "StorageChangeRate", componentDefinition.Value<float?>("storageChangeRate"));
            SetWritableMember(serializedComponent, "MaxStorage", componentDefinition.Value<float?>("maxStorage"));
            SetWritableMember(serializedComponent, "OrderAroundEmpties", componentDefinition.Value<bool?>("orderAroundEmpties"));
            SetWritableMember(serializedComponent, "CarTransferRate", componentDefinition.Value<float?>("carTransferRate"));
            SetWritableMember(serializedComponent, "OrderAroundLoaded", componentDefinition.Value<bool?>("orderAroundLoaded"));
            SetWritableMember(serializedComponent, "InputSpans", componentDefinition["inputSpans"] is JArray inputSpans ? inputSpans.Values<string>().Where(value => !string.IsNullOrWhiteSpace(value)).ToArray() : Array.Empty<string>());
            SetWritableMember(serializedComponent, "OutputSpans", componentDefinition["outputSpans"] is JArray outputSpans ? outputSpans.Values<string>().Where(value => !string.IsNullOrWhiteSpace(value)).ToArray() : Array.Empty<string>());
            SetWritableMember(serializedComponent, "CarLoadPeriod", componentDefinition.Value<float?>("carLoadPeriod"));
            SetWritableMember(serializedComponent, "CarLengthFeet", componentDefinition.Value<float?>("carLengthFeet"));
            SetWritableMember(serializedComponent, "CanOverhaul", componentDefinition.Value<bool?>("canOverhaul"));
            SetWritableMember(serializedComponent, "InputTermsPerDay", componentDefinition["inputTermsPerDay"] is JObject inputTerms ? inputTerms.Properties().ToDictionary(property => property.Name, property => property.Value.Value<float>(), StringComparer.OrdinalIgnoreCase) : null);
            SetWritableMember(serializedComponent, "OutputTermsPerDay", componentDefinition["outputTermsPerDay"] is JObject outputTerms ? outputTerms.Properties().ToDictionary(property => property.Name, property => property.Value.Value<float>(), StringComparer.OrdinalIgnoreCase) : null);

            var extraData = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
            var handledKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "type",
                "name",
                "trackSpans",
                "carTypeFilter",
                "sharedStorage",
                "loadId",
                "storageChangeRate",
                "maxStorage",
                "orderAroundEmpties",
                "carTransferRate",
                "orderAroundLoaded",
                "inputSpans",
                "outputSpans",
                "carLoadPeriod",
                "carLengthFeet",
                "inputTermsPerDay",
                "outputTermsPerDay",
                "canOverhaul"
            };

            foreach (var property in componentDefinition.Properties())
            {
                if (!handledKeys.Contains(property.Name))
                {
                    extraData[property.Name] = property.Value.DeepClone();
                }
            }

            SetWritableMember(serializedComponent, "ExtraData", extraData);
            return serializedComponent;
        }

        private static object BuildPatchingContext(Dictionary<string, Load> loads)
        {
            var patchingContextType = ResolveRuntimeType("StrangeCustoms.Tracks.PatchingContext");
            if (patchingContextType == null)
            {
                return null;
            }

            var loadResolver = new Func<string, Load>(loadId => ResolveLoad(loadId, loads));
            var constructor = patchingContextType.GetConstructor(new[] { typeof(Func<string, Load>) });
            if (constructor != null)
            {
                return constructor.Invoke(new object[] { loadResolver });
            }

            return Activator.CreateInstance(patchingContextType);
        }

        private static Dictionary<string, Load> ResolveLoads()
        {
            return (CarPrototypeLibrary.instance != null ? CarPrototypeLibrary.instance.opsLoads : Array.Empty<Load>())
                .Where(load => load != null && !string.IsNullOrWhiteSpace(load.id))
                .ToDictionary(load => load.id, StringComparer.OrdinalIgnoreCase);
        }

        private static Load ResolveLoad(string loadId, Dictionary<string, Load> loads)
        {
            if (string.IsNullOrWhiteSpace(loadId) || loads == null)
            {
                return null;
            }

            loads.TryGetValue(loadId, out var load);
            return load;
        }

        private static TrackSpan[] ResolveTrackSpans(JArray array, Dictionary<string, TrackSpan> spans)
        {
            if (array == null || spans == null)
            {
                return Array.Empty<TrackSpan>();
            }

            return array.Values<string>().Where(id => !string.IsNullOrWhiteSpace(id) && spans.ContainsKey(id)).Select(id => spans[id]).Distinct().ToArray();
        }

        private static bool TryParseLocation(JObject value, Dictionary<string, TrackSegment> segments, out Location? location)
        {
            location = null;
            if (value == null || segments == null)
            {
                return false;
            }

            var segmentId = value.Value<string>("segmentId");
            if (string.IsNullOrWhiteSpace(segmentId) || !segments.TryGetValue(segmentId, out var segment) || segment == null)
            {
                return false;
            }

            var endValue = value.Value<string>("end") ?? string.Empty;
            var end = string.Equals(endValue, "start", StringComparison.OrdinalIgnoreCase) || string.Equals(endValue, "a", StringComparison.OrdinalIgnoreCase)
                ? TrackSegment.End.A
                : TrackSegment.End.B;
            location = new Location(segment, value.Value<float?>("distance") ?? 0f, end);
            return true;
        }

        private static Transform GetOrCreateRoot(string name, Transform parent)
        {
            if (parent != null)
            {
                var existing = parent.Find(name);
                if (existing != null)
                {
                    return existing;
                }
            }

            var gameObject = new GameObject(name);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent, false);
            }
            return gameObject.transform;
        }

        private static void RebuildMapTerrain(Action<string> log)
        {
            if (MapManagerInstanceProperty == null || MapManagerRebuildAllMethod == null)
            {
                return;
            }

            try
            {
                var instance = MapManagerInstanceProperty.GetValue(null, null);
                if (instance == null)
                {
                    return;
                }

                MapManagerRebuildAllMethod.Invoke(instance, null);
                log?.Invoke("Requested full map terrain rebuild after world patch apply.");
            }
            catch (Exception ex)
            {
                log?.Invoke("Full map terrain rebuild failed: " + ex.Message);
            }
        }

        private static Type ResolveRuntimeType(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static void SetWritableMember(object target, string name, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            var type = target.GetType();
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value, null);
                return;
            }

            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private sealed class NativeTurntableMarker : MonoBehaviour
        {
        }

        private static JObject CreateVectorToken(Vector3 value)
        {
            return new JObject
            {
                ["x"] = value.x,
                ["y"] = value.y,
                ["z"] = value.z
            };
        }

        private static void ApplyTransform(Transform transform, JObject position, JObject rotation, bool useLocal)
        {
            if (position != null)
            {
                var vector = ParseVector(position);
                if (useLocal)
                {
                    transform.localPosition = vector;
                }
                else
                {
                    transform.position = vector;
                }
            }

            if (rotation != null)
            {
                var euler = ParseVector(rotation);
                if (useLocal)
                {
                    transform.localRotation = Quaternion.Euler(euler);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(euler);
                }
            }
        }

        private static void ApplyScale(Transform transform, JObject scale)
        {
            if (scale != null)
            {
                transform.localScale = ParseVector(scale);
            }
        }

        private static Vector3 ParseVector(JObject value)
        {
            return value == null ? Vector3.zero : new Vector3(value.Value<float?>("x") ?? 0f, value.Value<float?>("y") ?? 0f, value.Value<float?>("z") ?? 0f);
        }

        private static Color ParseColor(JToken value, Color fallback)
        {
            if (value is JObject map)
            {
                return new Color(map.Value<float?>("r") ?? fallback.r, map.Value<float?>("g") ?? fallback.g, map.Value<float?>("b") ?? fallback.b, map.Value<float?>("a") ?? fallback.a);
            }

            if (value is JArray array && array.Count >= 3)
            {
                return new Color(array[0].Value<float>(), array[1].Value<float>(), array[2].Value<float>(), array.Count >= 4 ? array[3].Value<float>() : fallback.a);
            }

            return fallback;
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            return Enum.TryParse(value, true, out TEnum parsed) ? parsed : fallback;
        }
    }
}
