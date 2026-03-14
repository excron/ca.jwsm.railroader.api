using System;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Persistence.Events;

namespace Ca.Jwsm.Railroader.Api.Persistence.Contracts
{
    public interface ISaveLifecycleService
    {
        IEventSubscription Subscribe(Action<SaveLifecycleEvent> handler);
    }
}
