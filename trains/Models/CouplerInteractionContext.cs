namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerInteractionContext
    {
        public CouplerInteractionContext(CouplerEndId couplerEndId, bool isFront, string displayName, object nativeVehicle = null, string nativeLogicalEnd = null)
        {
            CouplerEndId = couplerEndId;
            IsFront = isFront;
            DisplayName = displayName ?? string.Empty;
            NativeVehicle = nativeVehicle;
            NativeLogicalEnd = nativeLogicalEnd ?? string.Empty;
        }

        public CouplerEndId CouplerEndId { get; }

        public bool IsFront { get; }

        public string DisplayName { get; }

        public object NativeVehicle { get; }

        public string NativeLogicalEnd { get; }
    }
}
