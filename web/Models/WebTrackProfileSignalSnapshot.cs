namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTrackProfileSignalSnapshot
    {
        public WebTrackProfileSignalSnapshot(string id, string name, string aspect, float distanceMeters, float x, float z)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            Aspect = aspect ?? string.Empty;
            DistanceMeters = distanceMeters;
            X = x;
            Z = z;
        }

        public string Id { get; }

        public string Name { get; }

        public string Aspect { get; }

        public float DistanceMeters { get; }

        public float X { get; }

        public float Z { get; }
    }
}
