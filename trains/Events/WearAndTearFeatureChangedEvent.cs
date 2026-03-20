namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class WearAndTearFeatureChangedEvent
    {
        public WearAndTearFeatureChangedEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; }
    }
}
