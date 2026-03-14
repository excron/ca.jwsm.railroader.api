namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class ControlRequestResult
    {
        public ControlRequestResult(float acceptedValue, bool isBlocked = false, string reason = null)
        {
            AcceptedValue = acceptedValue;
            IsBlocked = isBlocked;
            Reason = reason;
        }

        public float AcceptedValue { get; }

        public bool IsBlocked { get; }

        public string Reason { get; }

        public static ControlRequestResult PassThrough(ControlRequest request)
        {
            return new ControlRequestResult(request.RequestedValue);
        }
    }
}
