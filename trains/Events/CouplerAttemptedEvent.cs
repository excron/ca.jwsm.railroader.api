using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class CouplerAttemptedEvent
    {
        public CouplerAttemptedEvent(CouplerEndId first, CouplerEndId second, float relativeSpeedMph = 0f)
        {
            First = first;
            Second = second;
            RelativeSpeedMph = relativeSpeedMph;
        }

        public CouplerEndId First { get; }

        public CouplerEndId Second { get; }

        public float RelativeSpeedMph { get; }

        public bool ShouldDisconnect { get; private set; }

        public string DisconnectReason { get; private set; }

        public void RequestDisconnect(string reason = null)
        {
            ShouldDisconnect = true;
            DisconnectReason = string.IsNullOrWhiteSpace(reason) ? "disconnect" : reason;
        }
    }
}
