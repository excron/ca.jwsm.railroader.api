using Ca.Jwsm.Railroader.Api.Abstractions.Events;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface ICouplerInteractionService
    {
        IEventSubscription Register(ICouplerInteractionProvider provider);
    }
}
