namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class ConstraintTelemetryCapturedEvent
    {
        public ConstraintTelemetryCapturedEvent(object nativeIntegrationSet, float deltaTimeSeconds, float[] deltaSeparationMeters)
        {
            NativeIntegrationSet = nativeIntegrationSet;
            DeltaTimeSeconds = deltaTimeSeconds;
            DeltaSeparationMeters = deltaSeparationMeters ?? new float[0];
        }

        public object NativeIntegrationSet { get; }

        public float DeltaTimeSeconds { get; }

        public float[] DeltaSeparationMeters { get; }
    }
}
