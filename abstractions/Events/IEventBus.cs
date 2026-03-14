using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Events
{
    public interface IEventBus
    {
        IEventSubscription Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;

        void Publish<TEvent>(TEvent eventInstance) where TEvent : class;
    }
}
