using System;
using System.IO;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Persistence.Contracts;
using Ca.Jwsm.Railroader.Api.Persistence.Events;
using Ca.Jwsm.Railroader.Api.Persistence.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class SaveContextService : ISaveContextService, ISaveLifecycleService
    {
        private readonly IEventBus _events;
        private readonly object _sync = new object();
        private SaveContext _current = SaveContext.Empty;

        public SaveContextService(IEventBus events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _events = events;
        }

        public SaveContext Current
        {
            get
            {
                lock (_sync)
                {
                    return _current;
                }
            }
        }

        public bool TryGetCurrent(out SaveContext context)
        {
            lock (_sync)
            {
                context = _current;
                return !string.IsNullOrWhiteSpace(_current.SaveId);
            }
        }

        public void SetCurrent(SaveContext context)
        {
            lock (_sync)
            {
                _current = context ?? SaveContext.Empty;
            }
        }

        public IEventSubscription Subscribe(Action<SaveLifecycleEvent> handler)
        {
            return _events.Subscribe(handler);
        }

        public void Publish(SaveLifecycleStage stage, string saveId, string displayName = null)
        {
            var context = CreateContext(saveId, displayName);

            lock (_sync)
            {
                _current = context;
            }

            _events.Publish(new SaveLifecycleEvent(stage, context));
        }

        public void Publish(SaveLifecycleStage stage)
        {
            SaveContext context;

            lock (_sync)
            {
                context = _current;
            }

            _events.Publish(new SaveLifecycleEvent(stage, context));
        }

        private static SaveContext CreateContext(string saveId, string displayName)
        {
            string resolvedId = string.IsNullOrWhiteSpace(saveId) ? string.Empty : saveId;
            string resolvedName = !string.IsNullOrWhiteSpace(displayName)
                ? displayName
                : (!string.IsNullOrWhiteSpace(resolvedId) ? Path.GetFileNameWithoutExtension(resolvedId) : string.Empty);
            return new SaveContext(resolvedId, resolvedName);
        }
    }
}
