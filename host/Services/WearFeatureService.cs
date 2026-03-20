using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Events;
using Game.State;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class WearFeatureService : IWearFeatureService
    {
        private readonly IEventBus _events;
        private bool? _lastPublishedState;

        public WearFeatureService(IEventBus events)
        {
            _events = events;
        }

        public bool IsWearAndTearEnabled
        {
            get { return ReadCurrentState(); }
        }

        public void Tick()
        {
            bool currentState = ReadCurrentState();
            if (_lastPublishedState.HasValue && _lastPublishedState.Value == currentState)
            {
                return;
            }

            _lastPublishedState = currentState;
            _events.Publish(new WearAndTearFeatureChangedEvent(currentState));
        }

        private static bool ReadCurrentState()
        {
            try
            {
                return StateManager.Shared?.Storage?.WearFeature ?? true;
            }
            catch
            {
                return true;
            }
        }
    }
}
