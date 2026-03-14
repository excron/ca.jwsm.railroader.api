using System;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Orders.Events;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Orders.Contracts
{
    public interface IExecutionObserverService
    {
        IEventSubscription Subscribe(Action<OrderLifecycleEvent> handler);

        bool TryGetState(string subjectId, out ExecutionObservationState state);
    }
}
