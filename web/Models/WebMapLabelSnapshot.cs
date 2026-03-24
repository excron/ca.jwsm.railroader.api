namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebMapLabelSnapshot
    {
        public WebMapLabelSnapshot(
            string id,
            string name,
            string kind,
            float x,
            float z,
            float radius,
            int priority,
            string colorHex)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            Kind = kind ?? string.Empty;
            X = x;
            Z = z;
            Radius = radius;
            Priority = priority;
            ColorHex = colorHex ?? string.Empty;
        }

        public string Id { get; }

        public string Name { get; }

        public string Kind { get; }

        public float X { get; }

        public float Z { get; }

        public float Radius { get; }

        public int Priority { get; }

        public string ColorHex { get; }
    }
}
