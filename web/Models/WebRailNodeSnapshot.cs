namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebRailNodeSnapshot
    {
        public WebRailNodeSnapshot(string id, string name, float x, float y, float z, bool isThrown, bool isCtcSwitch)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            X = x;
            Y = y;
            Z = z;
            IsThrown = isThrown;
            IsCtcSwitch = isCtcSwitch;
        }

        public string Id { get; }

        public string Name { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public bool IsThrown { get; }

        public bool IsCtcSwitch { get; }
    }
}
