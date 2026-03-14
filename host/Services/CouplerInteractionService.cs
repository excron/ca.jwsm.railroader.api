using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class CouplerInteractionService : ICouplerInteractionService
    {
        private readonly List<ICouplerInteractionProvider> _providers = new List<ICouplerInteractionProvider>();
        private ICouplerInteractionProvider[] _snapshot = new ICouplerInteractionProvider[0];
        private readonly object _sync = new object();

        public IEventSubscription Register(ICouplerInteractionProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            lock (_sync)
            {
                if (_providers.Contains(provider))
                {
                    throw new InvalidOperationException("Coupler interaction provider is already registered.");
                }

                _providers.Add(provider);
                _snapshot = _providers.ToArray();
            }

            return new Subscription(() => Unregister(provider));
        }

        internal void PopulateTooltip(CouplerInteractionContext context, CouplerTooltipContent tooltip)
        {
            if (context == null || tooltip == null)
            {
                return;
            }

            foreach (var provider in Snapshot())
            {
                provider.PopulateTooltip(context, tooltip);
            }
        }

        internal void PopulateMenu(CouplerInteractionContext context, CouplerMenuContent menu)
        {
            if (context == null || menu == null)
            {
                return;
            }

            foreach (var provider in Snapshot())
            {
                provider.PopulateMenu(context, menu);
            }
        }

        private ICouplerInteractionProvider[] Snapshot()
        {
            lock (_sync)
            {
                return _snapshot;
            }
        }

        private void Unregister(ICouplerInteractionProvider provider)
        {
            lock (_sync)
            {
                _providers.Remove(provider);
                _snapshot = _providers.Count == 0 ? new ICouplerInteractionProvider[0] : _providers.ToArray();
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
