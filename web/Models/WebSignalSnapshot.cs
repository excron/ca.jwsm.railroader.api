namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebSignalSnapshot
    {
        public WebSignalSnapshot(
            string id,
            string displayName,
            string aspect,
            string direction,
            float x,
            float y,
            float z)
        {
            Id = id ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            Aspect = aspect ?? string.Empty;
            Direction = direction ?? string.Empty;
            X = x;
            Y = y;
            Z = z;
        }

        public string Id { get; }

        public string DisplayName { get; }

        public string Aspect { get; }

        public string Direction { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }
    }
}
