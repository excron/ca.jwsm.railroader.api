using System;
using System.IO;
using System.Text;
using Ca.Jwsm.Railroader.Api.Persistence.Contracts;
using Ca.Jwsm.Railroader.Api.Persistence.Models;
using Newtonsoft.Json;
using UnityEngine;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ModDataStore : IModDataStore
    {
        private readonly ISaveContextService _saveContext;

        public ModDataStore(ISaveContextService saveContext)
        {
            if (saveContext == null)
            {
                throw new ArgumentNullException(nameof(saveContext));
            }

            _saveContext = saveContext;
        }

        public bool TryLoadJson(string ownerId, ModDataScope scope, ModDataKey key, out string json)
        {
            if (!TryGetPath(ownerId, scope, key, out var path))
            {
                json = null;
                return false;
            }

            if (!File.Exists(path))
            {
                json = null;
                return false;
            }

            json = File.ReadAllText(path, Encoding.UTF8);
            return true;
        }

        public void SaveJson(string ownerId, ModDataScope scope, ModDataKey key, string json)
        {
            string path = GetRequiredPath(ownerId, scope, key);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json ?? string.Empty, Encoding.UTF8);
        }

        public bool TryLoad<T>(string ownerId, ModDataScope scope, ModDataKey key, out T value)
        {
            if (!TryLoadJson(ownerId, scope, key, out var json) || string.IsNullOrWhiteSpace(json))
            {
                value = default(T);
                return false;
            }

            value = JsonConvert.DeserializeObject<T>(json);
            return value != null;
        }

        public void Save<T>(string ownerId, ModDataScope scope, ModDataKey key, T value)
        {
            SaveJson(ownerId, scope, key, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        public void Delete(string ownerId, ModDataScope scope, ModDataKey key, string scopeId = null)
        {
            string path = GetRequiredPath(ownerId, scope, key, scopeId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string root = Path.Combine(Application.persistentDataPath, "Mods", Sanitize(ownerId));
            DeleteEmptyDirectoryChain(Path.GetDirectoryName(path), root);
        }

        private string GetRequiredPath(string ownerId, ModDataScope scope, ModDataKey key, string scopeId = null)
        {
            if (!TryGetPath(ownerId, scope, key, out var path, scopeId))
            {
                throw new InvalidOperationException("Save-scoped mod data requires an active save context or an explicit scope id.");
            }

            return path;
        }

        private bool TryGetPath(string ownerId, ModDataScope scope, ModDataKey key, out string path, string scopeId = null)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException("Owner id is required.", nameof(ownerId));
            }

            string root = Path.Combine(Application.persistentDataPath, "Mods", Sanitize(ownerId));
            switch (scope)
            {
                case ModDataScope.Global:
                    path = Path.Combine(root, "global", Sanitize(key.Value) + ".json");
                    return true;
                case ModDataScope.Save:
                    string saveId = !string.IsNullOrWhiteSpace(scopeId)
                        ? scopeId
                        : TryResolveCurrentSaveId();
                    if (string.IsNullOrWhiteSpace(saveId))
                    {
                        path = string.Empty;
                        return false;
                    }

                    path = Path.Combine(root, "saves", Sanitize(saveId), Sanitize(key.Value) + ".json");
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
        }

        private string TryResolveCurrentSaveId()
        {
            return _saveContext.TryGetCurrent(out var context) && !string.IsNullOrWhiteSpace(context.SaveId)
                ? context.SaveId
                : string.Empty;
        }

        private static string Sanitize(string value)
        {
            string result = value ?? string.Empty;
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                result = result.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(result) ? "default" : result;
        }

        private static void DeleteEmptyDirectoryChain(string startDirectory, string stopAtDirectoryInclusive)
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
            {
                return;
            }

            string current = startDirectory;
            string stopAt = string.IsNullOrWhiteSpace(stopAtDirectoryInclusive)
                ? string.Empty
                : Path.GetFullPath(stopAtDirectoryInclusive.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            while (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current))
            {
                if (Directory.GetFiles(current).Length != 0 || Directory.GetDirectories(current).Length != 0)
                {
                    break;
                }

                Directory.Delete(current, recursive: false);
                string fullCurrent = Path.GetFullPath(current.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (!string.IsNullOrWhiteSpace(stopAt) && string.Equals(fullCurrent, stopAt, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                current = Path.GetDirectoryName(current);
            }
        }
    }
}
