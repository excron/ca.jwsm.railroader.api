namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class WearAndTearFeatureChangedEvent
    {
        public WearAndTearFeatureChangedEvent(bool isEnabled, float wearMultiplier, int overhaulMiles)
        {
            IsEnabled = isEnabled;
            WearMultiplier = wearMultiplier;
            OverhaulMiles = overhaulMiles;
        }

        public bool IsEnabled { get; }

        public float WearMultiplier { get; }

        public int OverhaulMiles { get; }
    }
}
