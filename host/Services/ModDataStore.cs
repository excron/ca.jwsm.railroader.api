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
            string path = GetPath(ownerId, scope, key);
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
            string path = GetPath(ownerId, scope, key);
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
            SaveJson(ownerId, scope, key, JsonConvert.SerializeObject(value));
        }

        public void Delete(string ownerId, ModDataScope scope, ModDataKey key, string scopeId = null)
        {
            string path = GetPath(ownerId, scope, key, scopeId);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
            {
                Directory.Delete(directory, recursive: false);
            }
        }

        private string GetPath(string ownerId, ModDataScope scope, ModDataKey key, string scopeId = null)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                throw new ArgumentException("Owner id is required.", nameof(ownerId));
            }

            string root = Path.Combine(Application.persistentDataPath, "Mods", Sanitize(ownerId));
            switch (scope)
            {
                case ModDataScope.Global:
                    return Path.Combine(root, "global", Sanitize(key.Value) + ".json");
                case ModDataScope.Save:
                    string saveId = !string.IsNullOrWhiteSpace(scopeId)
                        ? scopeId
                        : (_saveContext.TryGetCurrent(out var context) && !string.IsNullOrWhiteSpace(context.SaveId)
                            ? context.SaveId
                            : "default");
                    return Path.Combine(root, "saves", Sanitize(saveId), Sanitize(key.Value) + ".json");
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope));
            }
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
    }
}
