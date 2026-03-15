using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ca.Jwsm.Railroader.Api.Host.World
{
    internal static class WorldNotificationBridge
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

        internal static void PublishGraphJsonWillDeserialize(JObject root, IDictionary<string, string> changedKeys, Action<string> log)
        {
            if (root == null)
            {
                return;
            }

            var eventType = ResolveType("StrangeCustoms.GraphJsonWillDeserializeEvent");
            if (eventType == null)
            {
                return;
            }

            try
            {
                var constructor = eventType.GetConstructor(new[]
                {
                    typeof(IDictionary<string, string>),
                    typeof(Action<string, JObject>)
                });
                if (constructor == null)
                {
                    return;
                }

                var message = constructor.Invoke(new object[]
                {
                    changedKeys,
                    new Action<string, JObject>((patchSource, patch) =>
                    {
                        if (patch == null)
                        {
                            return;
                        }

                        WorldLayoutDefinition.MergeRootPatch(root, patch, patchSource, changedKeys);
                    })
                });

                TrySend(message, eventType, log);
            }
            catch (Exception ex)
            {
                log?.Invoke("World notification bridge failed to publish GraphJsonWillDeserialize: " + ex.Message);
            }
        }

        internal static void PublishGraphWillChange(JObject root, IEnumerable<string> changedPaths, Action<string> log)
        {
            var eventType = ResolveType("StrangeCustoms.GraphWillChangeEvent");
            var pathSegmentType = ResolveType("StrangeCustoms.Tracks.PathSegment");
            var trackStateType = ResolveType("StrangeCustoms.Tracks.TrackState");
            if (eventType == null || pathSegmentType == null || trackStateType == null)
            {
                return;
            }

            try
            {
                var pathSegments = CreatePathSegments(pathSegmentType, changedPaths);
                var state = BuildTrackState(trackStateType, root);
                var constructor = eventType.GetConstructor(new[] { trackStateType, pathSegments.GetType() });
                if (constructor == null)
                {
                    return;
                }

                var message = constructor.Invoke(new object[] { state, pathSegments });
                TrySend(message, eventType, log);
            }
            catch (Exception ex)
            {
                log?.Invoke("World notification bridge failed to publish GraphWillChange: " + ex.Message);
            }
        }

        internal static void PublishGraphDidChange(JObject root, Action<string> log)
        {
            var eventType = ResolveType("StrangeCustoms.GraphDidChangeEvent");
            var trackStateType = ResolveType("StrangeCustoms.Tracks.TrackState");
            if (eventType == null || trackStateType == null)
            {
                return;
            }

            try
            {
                var state = BuildTrackState(trackStateType, root);
                var constructor = eventType.GetConstructor(new[] { trackStateType });
                if (constructor == null)
                {
                    return;
                }

                var message = constructor.Invoke(new[] { state });
                TrySend(message, eventType, log);
            }
            catch (Exception ex)
            {
                log?.Invoke("World notification bridge failed to publish GraphDidChange: " + ex.Message);
            }
        }

        internal static void PublishIndustriesDidChange(Action<string> log)
        {
            var eventType = ResolveType("Game.Events.IndustriesDidChange");
            if (eventType == null)
            {
                return;
            }

            try
            {
                var message = Activator.CreateInstance(eventType);
                TrySend(message, eventType, log);
            }
            catch (Exception ex)
            {
                log?.Invoke("World notification bridge failed to publish IndustriesDidChange: " + ex.Message);
            }
        }

        private static object BuildTrackState(Type trackStateType, JObject root)
        {
            var state = Activator.CreateInstance(trackStateType);
            if (state == null || root == null)
            {
                return state;
            }

            var graphTracksType = ResolveType("StrangeCustoms.Tracks.GraphTracks");
            if (graphTracksType != null)
            {
                var tracks = Activator.CreateInstance(graphTracksType);
                SetMemberValue(tracks, "Nodes", DeserializeDictionary(ResolveSection(root, "tracks", "nodes"), ResolveType("StrangeCustoms.Tracks.SerializedNode")));
                SetMemberValue(tracks, "Segments", DeserializeDictionary(ResolveSection(root, "tracks", "segments"), ResolveType("StrangeCustoms.Tracks.SerializedSegment")));
                SetMemberValue(tracks, "Spans", DeserializeDictionary(ResolveSection(root, "tracks", "spans"), ResolveType("StrangeCustoms.Tracks.SerializedSpan")));
                SetMemberValue(state, "Tracks", tracks);
            }

            SetMemberValue(state, "Areas", DeserializeDictionary(ResolveSection(root, "areas"), ResolveType("StrangeCustoms.Tracks.SerializedArea")));
            SetMemberValue(state, "Loads", DeserializeDictionary(ResolveSection(root, "loads"), ResolveType("StrangeCustoms.Tracks.SerializedLoad")));
            SetMemberValue(state, "Texts", DeserializeDictionary(ResolveSection(root, "texts"), typeof(string)));
            SetMemberValue(state, "Scenery", DeserializeDictionary(ResolveSection(root, "scenery"), ResolveType("StrangeCustoms.Tracks.SerializedScenery")));
            SetMemberValue(state, "Splineys", DeserializeDictionary(ResolveSection(root, "splineys"), typeof(JObject)));
            SetMemberValue(state, "SimpleGraphs", DeserializeDictionary(ResolveSection(root, "simpleGraphs"), ResolveType("StrangeCustoms.Tracks.SerializedSimpleGraph")));
            SetMemberValue(state, "Mandelas", DeserializeDictionary(ResolveSection(root, "mandelas"), ResolveType("StrangeCustoms.Tracks.Mandela")));
            return state;
        }

        private static object DeserializeDictionary(JObject section, Type valueType)
        {
            if (valueType == null)
            {
                return null;
            }

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), valueType);
            if (section == null || !section.Properties().Any())
            {
                return Activator.CreateInstance(dictionaryType);
            }

            try
            {
                return section.ToObject(dictionaryType, Serializer) ?? Activator.CreateInstance(dictionaryType);
            }
            catch
            {
                return Activator.CreateInstance(dictionaryType);
            }
        }

        private static object CreatePathSegments(Type pathSegmentType, IEnumerable<string> changedPaths)
        {
            var listType = typeof(List<>).MakeGenericType(pathSegmentType);
            var list = Activator.CreateInstance(listType);
            var addMethod = listType.GetMethod("Add");
            var createMethod = pathSegmentType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
            if (list == null || addMethod == null || createMethod == null)
            {
                return Activator.CreateInstance(listType);
            }

            foreach (var path in (changedPaths ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var segment = createMethod.Invoke(null, new object[] { path });
                addMethod.Invoke(list, new[] { segment });
            }

            return list;
        }

        private static void SetMemberValue(object target, string name, object value)
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

        private static JObject ResolveSection(JObject root, params string[] path)
        {
            JToken current = root;
            for (var index = 0; index < path.Length; index++)
            {
                current = current?[path[index]];
                if (current == null)
                {
                    return null;
                }
            }

            return current as JObject;
        }

        private static Type ResolveType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName, false))
                .FirstOrDefault(type => type != null);
        }

        private static void TrySend(object message, Type messageType, Action<string> log)
        {
            if (message == null || messageType == null)
            {
                return;
            }

            try
            {
                var messengerType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(assembly => assembly.GetType("GalaSoft.MvvmLight.Messaging.Messenger", false))
                    .FirstOrDefault(type => type != null);
                if (messengerType == null)
                {
                    return;
                }

                var messenger = messengerType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null, null);
                if (messenger == null)
                {
                    return;
                }

                var sendMethod = messengerType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(method =>
                        string.Equals(method.Name, "Send", StringComparison.Ordinal) &&
                        method.IsGenericMethodDefinition &&
                        method.GetParameters().Length == 1);
                if (sendMethod == null)
                {
                    return;
                }

                sendMethod.MakeGenericMethod(messageType).Invoke(messenger, new[] { message });
            }
            catch (Exception ex)
            {
                log?.Invoke("World notification bridge dispatch failed: " + ex.Message);
            }
        }
    }
}
