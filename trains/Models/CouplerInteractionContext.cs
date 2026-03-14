namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerInteractionContext
    {
        public CouplerInteractionContext(CouplerEndId couplerEndId, bool isFront, string displayName)
        {
            CouplerEndId = couplerEndId;
            IsFront = isFront;
            DisplayName = displayName ?? string.Empty;
        }

        public CouplerEndId CouplerEndId { get; }

        public bool IsFront { get; }

        public string DisplayName { get; }
    }
}
