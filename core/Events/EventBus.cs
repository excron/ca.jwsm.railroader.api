using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;

namespace Ca.Jwsm.Railroader.Api.Core.Events
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();
        private readonly Dictionary<Type, Action<object>[]> _snapshots = new Dictionary<Type, Action<object>[]>();
        private readonly object _sync = new object();

        public IEventSubscription Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Action<object> wrapper = value => handler((TEvent)value);

            lock (_sync)
            {
                if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
                {
                    handlers = new List<Action<object>>();
                    _handlers.Add(typeof(TEvent), handlers);
                }

                handlers.Add(wrapper);
                _snapshots[typeof(TEvent)] = handlers.ToArray();
            }

            return new Subscription(() => Unsubscribe(typeof(TEvent), wrapper));
        }

        public bool HasSubscribers<TEvent>() where TEvent : class
        {
            lock (_sync)
            {
                return _snapshots.TryGetValue(typeof(TEvent), out var handlers) && handlers.Length > 0;
            }
        }

        public void Publish<TEvent>(TEvent eventInstance) where TEvent : class
        {
            if (eventInstance == null)
            {
                throw new ArgumentNullException(nameof(eventInstance));
            }

            Action<object>[] handlers;

            lock (_sync)
            {
                if (!_snapshots.TryGetValue(typeof(TEvent), out handlers) || handlers.Length == 0)
                {
                    return;
                }
            }

            for (int i = 0; i < handlers.Length; i++)
            {
                handlers[i](eventInstance);
            }
        }

        private void Unsubscribe(Type eventType, Action<object> handler)
        {
            lock (_sync)
            {
                if (!_handlers.TryGetValue(eventType, out var handlers))
                {
                    return;
                }

                handlers.Remove(handler);

                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                    _snapshots.Remove(eventType);
                }
                else
                {
                    _snapshots[eventType] = handlers.ToArray();
                }
            }
        }

        private sealed class Subscription : IEventSubscription
        {
            private readonly Action _dispose;
            private bool _disposed;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _dispose();
                _disposed = true;
            }
        }
    }
}
