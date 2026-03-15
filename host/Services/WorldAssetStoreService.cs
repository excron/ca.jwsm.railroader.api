using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetPack.Runtime;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Models;
using Model;
using Model.Database;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class WorldAssetStoreService : IWorldAssetStoreService
    {
        private readonly object _sync = new object();
        private readonly Dictionary<string, Dictionary<string, WorldAssetStoreRegistration>> _registrationsBySource =
            new Dictionary<string, Dictionary<string, WorldAssetStoreRegistration>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _resolvedBasePaths =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Reflection.MethodInfo _prefabStoreAddStoreMethod =
            typeof(PrefabStore).GetMethod("AddStore", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public WorldAssetStoreService()
        {
            Status = new WorldAssetStoreStatus(0, 0, "Waiting for external world asset stores.");
        }

        public WorldAssetStoreStatus Status { get; private set; }

        public void Update(string sourceId, IReadOnlyList<WorldAssetStoreRegistration> registrations)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return;
            }

            lock (_sync)
            {
                var normalized = new Dictionary<string, WorldAssetStoreRegistration>(StringComparer.OrdinalIgnoreCase);
                foreach (var registration in registrations ?? Array.Empty<WorldAssetStoreRegistration>())
                {
                    if (registration == null ||
                        string.IsNullOrWhiteSpace(registration.Identifier) ||
                        string.IsNullOrWhiteSpace(registration.BasePath))
                    {
                        continue;
                    }

                    normalized[registration.Identifier] = registration;
                }

                _registrationsBySource[sourceId] = normalized;
                RebuildResolvedPaths();
            }

            EnsureRegisteredForCurrentStore();
        }

        public void Clear(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return;
            }

            lock (_sync)
            {
                if (_registrationsBySource.Remove(sourceId))
                {
                    RebuildResolvedPaths();
                }
            }
        }

        internal bool TryResolveBasePath(string identifier, out string basePath)
        {
            lock (_sync)
            {
                return _resolvedBasePaths.TryGetValue(identifier ?? string.Empty, out basePath);
            }
        }

        internal void EnsureRegisteredForCurrentStore()
        {
            var prefabStore = TrainController.Shared != null ? TrainController.Shared.PrefabStore as PrefabStore : null;
            if (prefabStore == null)
            {
                return;
            }

            RegisterStores(prefabStore);
        }

        internal void RegisterStores(PrefabStore prefabStore)
        {
            if (prefabStore == null || _prefabStoreAddStoreMethod == null)
            {
                return;
            }

            string[] identifiers;
            lock (_sync)
            {
                identifiers = _resolvedBasePaths.Keys
                    .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }

            if (identifiers.Length == 0)
            {
                return;
            }

            var existingStores = new HashSet<string>(
                prefabStore.ExternalStores.Select(store => store.Identifier),
                StringComparer.OrdinalIgnoreCase);

            foreach (var identifier in identifiers)
            {
                if (existingStores.Contains(identifier))
                {
                    continue;
                }

                _prefabStoreAddStoreMethod.Invoke(prefabStore, new object[] { identifier, AssetPackRuntimeStore.StoreLocation.External });
                existingStores.Add(identifier);
            }
        }

        private void RebuildResolvedPaths()
        {
            _resolvedBasePaths.Clear();

            foreach (var source in _registrationsBySource.Values)
            {
                foreach (var pair in source)
                {
                    _resolvedBasePaths[pair.Key] = pair.Value.BasePath;
                }
            }

            Status = new WorldAssetStoreStatus(
                _registrationsBySource.Count,
                _resolvedBasePaths.Count,
                _resolvedBasePaths.Count == 0
                    ? "Waiting for external world asset stores."
                    : "Registered " + _resolvedBasePaths.Count + " external world asset store(s) from " + _registrationsBySource.Count + " source(s).");
        }
    }
}
