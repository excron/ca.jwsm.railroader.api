using System;
using System.Collections.Generic;
using System.Linq;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Models;
using Newtonsoft.Json.Linq;

namespace Ca.Jwsm.Railroader.Api.Host.World
{
    internal sealed class WorldLayoutDefinition
    {
        internal JObject Root { get; } = new JObject();
        internal List<string> Sources { get; } = new List<string>();
        internal Dictionary<string, string> ChangedKeys { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        internal JObject Nodes => ResolveOrCreateSection(Root, "tracks", "nodes");
        internal JObject Segments => ResolveOrCreateSection(Root, "tracks", "segments");
        internal JObject Spans => ResolveOrCreateSection(Root, "tracks", "spans");
        internal JObject Areas => ResolveOrCreateSection(Root, "areas");
        internal JObject Loads => ResolveOrCreateSection(Root, "loads");
        internal JObject Texts => ResolveOrCreateSection(Root, "texts");
        internal JObject Scenery => ResolveOrCreateSection(Root, "scenery");
        internal JObject Splineys => ResolveOrCreateSection(Root, "splineys");
        internal JObject SimpleGraphs => ResolveOrCreateSection(Root, "simpleGraphs");
        internal JObject Mandelas => ResolveOrCreateSection(Root, "mandelas");
        internal bool RequiresOps => Areas.Properties().Any();

        internal static WorldLayoutDefinition Build(WorldLayoutSourceUpdate update, Action<string> log)
        {
            var definition = new WorldLayoutDefinition();
            foreach (var document in (update != null ? update.Documents : Array.Empty<WorldLayoutDocument>()))
            {
                if (document == null || string.IsNullOrWhiteSpace(document.Json))
                {
                    continue;
                }

                var sourceName = string.IsNullOrWhiteSpace(document.SourcePath) ? "<memory>" : document.SourcePath;
                var root = JObject.Parse(document.Json);
                definition.Sources.Add(sourceName);
                MergeGraphPatch(definition.Root, root, sourceName, definition.ChangedKeys);
            }

            WorldNotificationBridge.PublishGraphJsonWillDeserialize(definition.Root, definition.ChangedKeys, log);
            return definition;
        }

        internal static void MergeRootPatch(JObject targetRoot, JObject patch, string source, IDictionary<string, string> changedKeys)
        {
            if (targetRoot == null || patch == null)
            {
                return;
            }

            MergeGraphPatch(targetRoot, patch, source, changedKeys);
        }

        private static void MergeGraphPatch(JObject targetRoot, JObject sourceRoot, string source, IDictionary<string, string> changedKeys)
        {
            if (targetRoot == null || sourceRoot == null)
            {
                return;
            }

            MergeSection(ResolveOrCreateSection(targetRoot, "tracks", "nodes"), ResolveSection(sourceRoot, "tracks", "nodes") ?? ResolveSection(sourceRoot, "nodes"), source, changedKeys, "tracks.nodes");
            MergeSection(ResolveOrCreateSection(targetRoot, "tracks", "segments"), ResolveSection(sourceRoot, "tracks", "segments") ?? ResolveSection(sourceRoot, "segments"), source, changedKeys, "tracks.segments");
            MergeSection(ResolveOrCreateSection(targetRoot, "tracks", "spans"), ResolveSection(sourceRoot, "tracks", "spans") ?? ResolveSection(sourceRoot, "spans"), source, changedKeys, "tracks.spans");
            MergeSection(ResolveOrCreateSection(targetRoot, "areas"), ResolveSection(sourceRoot, "areas"), source, changedKeys, "areas");
            MergeSection(ResolveOrCreateSection(targetRoot, "loads"), ResolveSection(sourceRoot, "loads"), source, changedKeys, "loads");
            MergeSection(ResolveOrCreateSection(targetRoot, "texts"), ResolveSection(sourceRoot, "texts"), source, changedKeys, "texts");
            MergeSection(ResolveOrCreateSection(targetRoot, "scenery"), ResolveSection(sourceRoot, "scenery"), source, changedKeys, "scenery");
            MergeSection(ResolveOrCreateSection(targetRoot, "splineys"), ResolveSection(sourceRoot, "splineys"), source, changedKeys, "splineys");
            MergeSection(ResolveOrCreateSection(targetRoot, "simpleGraphs"), ResolveSection(sourceRoot, "simpleGraphs"), source, changedKeys, "simpleGraphs");
            MergeSection(ResolveOrCreateSection(targetRoot, "mandelas"), ResolveSection(sourceRoot, "mandelas"), source, changedKeys, "mandelas");
        }

        private static JObject ResolveSection(JObject root, params string[] path)
        {
            JToken current = root;
            for (var index = 0; index < path.Length; index++)
            {
                current = current[path[index]];
                if (current == null)
                {
                    return null;
                }
            }

            return current as JObject;
        }

        private static JObject ResolveOrCreateSection(JObject root, params string[] path)
        {
            var current = root;
            for (var index = 0; index < path.Length; index++)
            {
                if (!(current[path[index]] is JObject next))
                {
                    next = new JObject();
                    current[path[index]] = next;
                }

                current = next;
            }

            return current;
        }

        private static void MergeSection(JObject target, JObject source, string sourceName, IDictionary<string, string> changedKeys, string pathPrefix)
        {
            if (target == null || source == null)
            {
                return;
            }

            foreach (var property in source.Properties())
            {
                var propertyPath = string.IsNullOrWhiteSpace(pathPrefix) ? property.Name : pathPrefix + "." + property.Name;
                RecordChangedKeys(changedKeys, property.Value, propertyPath, sourceName);
                if (target.TryGetValue(property.Name, out var existing))
                {
                    target[property.Name] = MergeToken(existing, property.Value);
                }
                else
                {
                    target[property.Name] = NormalizeToken(property.Value);
                }
            }
        }

        private static void RecordChangedKeys(IDictionary<string, string> changedKeys, JToken token, string path, string sourceName)
        {
            if (changedKeys == null || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            changedKeys[path] = sourceName ?? string.Empty;
            if (!(token is JObject obj))
            {
                return;
            }

            foreach (var property in obj.Properties())
            {
                RecordChangedKeys(changedKeys, property.Value, path + "." + property.Name, sourceName);
            }
        }

        private static JToken MergeToken(JToken existing, JToken incoming)
        {
            incoming = NormalizeToken(incoming);
            if (incoming == null)
            {
                return JValue.CreateNull();
            }

            if (incoming.Type == JTokenType.Null)
            {
                return JValue.CreateNull();
            }

            if (existing is JObject existingObject && incoming is JObject incomingObject)
            {
                var merged = (JObject)existingObject.DeepClone();
                foreach (var property in incomingObject.Properties())
                {
                    if (merged.TryGetValue(property.Name, out var nestedExisting))
                    {
                        merged[property.Name] = MergeToken(nestedExisting, property.Value);
                    }
                    else
                    {
                        merged[property.Name] = NormalizeToken(property.Value);
                    }
                }

                return merged;
            }

            return incoming.DeepClone();
        }

        private static JToken NormalizeToken(JToken token)
        {
            if (token is JObject obj)
            {
                if (obj.TryGetValue("$replace", out var replaceToken))
                {
                    return NormalizeToken(replaceToken);
                }

                var normalized = new JObject();
                foreach (var property in obj.Properties())
                {
                    normalized[property.Name] = NormalizeToken(property.Value);
                }

                return normalized;
            }

            if (token is JArray array)
            {
                var normalized = new JArray();
                foreach (var child in array)
                {
                    normalized.Add(NormalizeToken(child));
                }

                return normalized;
            }

            return token?.DeepClone();
        }
    }
}
