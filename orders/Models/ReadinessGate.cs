namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public sealed class ReadinessGate
    {
        public ReadinessGate(string name, bool isReady, string reason = null)
        {
            Name = name;
            IsReady = isReady;
            Reason = reason;
        }

        public string Name { get; }

        public bool IsReady { get; }

        public string Reason { get; }
    }
}
