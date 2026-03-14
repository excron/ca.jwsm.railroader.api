namespace Ca.Jwsm.Railroader.Api.Ui.Events
{
    public sealed class TrainBrakeDisplayAvailableEvent
    {
        public TrainBrakeDisplayAvailableEvent(object nativeDisplay)
        {
            NativeDisplay = nativeDisplay;
        }

        public object NativeDisplay { get; }
    }
}
