using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Events;
using Game.State;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class WearFeatureService : IWearFeatureService
    {
        private readonly IEventBus _events;
        private WearSnapshot? _lastPublishedState;

        private readonly struct WearSnapshot : System.IEquatable<WearSnapshot>
        {
            public WearSnapshot(bool isEnabled, float wearMultiplier, int overhaulMiles)
            {
                IsEnabled = isEnabled;
                WearMultiplier = wearMultiplier;
                OverhaulMiles = overhaulMiles;
            }

            public bool IsEnabled { get; }

            public float WearMultiplier { get; }

            public int OverhaulMiles { get; }

            public bool Equals(WearSnapshot other)
            {
                return IsEnabled == other.IsEnabled
                    && System.Math.Abs(WearMultiplier - other.WearMultiplier) < 0.0001f
                    && OverhaulMiles == other.OverhaulMiles;
            }
        }

        public WearFeatureService(IEventBus events)
        {
            _events = events;
        }

        public bool IsWearAndTearEnabled
        {
            get { return ReadCurrentState().IsEnabled; }
        }

        public float WearMultiplier
        {
            get { return ReadCurrentState().WearMultiplier; }
        }

        public int OverhaulMiles
        {
            get { return ReadCurrentState().OverhaulMiles; }
        }

        public void Tick()
        {
            var currentState = ReadCurrentState();
            if (_lastPublishedState.HasValue && _lastPublishedState.Value.Equals(currentState))
            {
                return;
            }

            _lastPublishedState = currentState;
            _events.Publish(new WearAndTearFeatureChangedEvent(currentState.IsEnabled, currentState.WearMultiplier, currentState.OverhaulMiles));
        }

        private static WearSnapshot ReadCurrentState()
        {
            try
            {
                var storage = StateManager.Shared?.Storage;
                bool isEnabled = storage?.WearFeature ?? true;
                float wearMultiplier = storage?.WearMultiplier ?? 1f;
                int overhaulMiles = storage?.OverhaulMiles ?? 2500;
                return new WearSnapshot(isEnabled, wearMultiplier, overhaulMiles);
            }
            catch
            {
                return new WearSnapshot(true, 1f, 2500);
            }
        }
    }
}
