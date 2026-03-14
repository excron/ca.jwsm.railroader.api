using System;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Orders.Contracts;
using Ca.Jwsm.Railroader.Api.Orders.Events;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ExecutionObserverService : IExecutionObserverService
    {
        private readonly IEventBus _eventBus;

        public ExecutionObserverService(IEventBus eventBus)
        {
            if (eventBus == null)
            {
                throw new ArgumentNullException(nameof(eventBus));
            }

            _eventBus = eventBus;
        }

        public IEventSubscription Subscribe(Action<OrderLifecycleEvent> handler)
        {
            return _eventBus.Subscribe(handler);
        }

        public bool TryGetState(string subjectId, out ExecutionObservationState state)
        {
            state = null;
            return false;
        }
    }
}
