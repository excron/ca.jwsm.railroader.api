using System;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Models;
using Ca.Jwsm.Railroader.Api.Host.World;
using Model;
using Model.Ops;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class WorldLayoutService : IWorldLayoutService
    {
        private WorldLayoutSourceUpdate _currentUpdate;
        private IWorldLayoutResolver _currentResolver;
        private int _appliedGraphInstanceId;

        public WorldLayoutService()
        {
            Status = new WorldLayoutStatus(false, string.Empty, 0, "Waiting for a submitted world layout.");
        }

        public WorldLayoutStatus Status { get; private set; }

        public void Update(WorldLayoutSourceUpdate update, IWorldLayoutResolver resolver)
        {
            if (update == null)
            {
                return;
            }

            if (_currentUpdate != null &&
                string.Equals(_currentUpdate.SourceId, update.SourceId, StringComparison.OrdinalIgnoreCase) &&
                _currentUpdate.Revision == update.Revision)
            {
                _currentResolver = resolver;
                return;
            }

            _currentUpdate = update;
            _currentResolver = resolver;
            _appliedGraphInstanceId = 0;
            Status = new WorldLayoutStatus(true, update.SourceId, update.Revision, "World layout update received. Waiting for a loaded map.");
        }

        public void Clear(string sourceId)
        {
            if (_currentUpdate == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(sourceId) && !string.Equals(_currentUpdate.SourceId, sourceId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _currentUpdate = null;
            _currentResolver = null;
            _appliedGraphInstanceId = 0;
            Status = new WorldLayoutStatus(false, string.Empty, 0, "Waiting for a submitted world layout.");
        }

        public void Tick()
        {
            TryApplyInternal(null);
        }

        public void TryApplyEarly(string reason)
        {
            TryApplyInternal(reason);
        }

        private void TryApplyInternal(string reason)
        {
            if (_currentUpdate == null)
            {
                return;
            }

            var graph = TrainController.Shared != null ? TrainController.Shared.graph : null;
            if (graph == null)
            {
                _appliedGraphInstanceId = 0;
                Status = new WorldLayoutStatus(true, _currentUpdate.SourceId, _currentUpdate.Revision, "Waiting for a loaded map.");
                return;
            }

            var graphInstanceId = graph.GetInstanceID();
            if (_appliedGraphInstanceId == graphInstanceId)
            {
                return;
            }

            try
            {
                var definition = WorldLayoutDefinition.Build(_currentUpdate, Log);
                if (definition.RequiresOps && OpsController.Shared == null)
                {
                    Status = new WorldLayoutStatus(true, _currentUpdate.SourceId, _currentUpdate.Revision, "Waiting for operations systems to finish loading.");
                    return;
                }

                var result = WorldLayoutApplier.Apply(definition, _currentResolver, Log);
                _appliedGraphInstanceId = graphInstanceId;
                Status = new WorldLayoutStatus(true, _currentUpdate.SourceId, _currentUpdate.Revision, result);
            }
            catch (Exception ex)
            {
                _appliedGraphInstanceId = graphInstanceId;
                var summary = "World layout apply failed" + (string.IsNullOrWhiteSpace(reason) ? string.Empty : " during " + reason) + ": " + ex.Message;
                Status = new WorldLayoutStatus(true, _currentUpdate.SourceId, _currentUpdate.Revision, summary);
            }
        }

        private void Log(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            Status = new WorldLayoutStatus(
                _currentUpdate != null,
                _currentUpdate != null ? _currentUpdate.SourceId : string.Empty,
                _currentUpdate != null ? _currentUpdate.Revision : 0,
                message);
        }
    }
}
