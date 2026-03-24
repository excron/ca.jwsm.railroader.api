namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebRailSegmentSnapshot
    {
        public WebRailSegmentSnapshot(
            string id,
            string name,
            string nodeAId,
            string nodeBId,
            float ax,
            float az,
            float bx,
            float bz,
            float length,
            float speedLimit,
            string groupId,
            bool isAvailable,
            bool isGroupEnabled,
            bool isInvisible,
            System.Collections.Generic.IReadOnlyList<WebRailPointSnapshot> points,
            string style,
            string trackClass)
        {
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
            NodeAId = nodeAId ?? string.Empty;
            NodeBId = nodeBId ?? string.Empty;
            Ax = ax;
            Az = az;
            Bx = bx;
            Bz = bz;
            Length = length;
            SpeedLimit = speedLimit;
            GroupId = groupId ?? string.Empty;
            IsAvailable = isAvailable;
            IsGroupEnabled = isGroupEnabled;
            IsInvisible = isInvisible;
            Points = points ?? System.Array.Empty<WebRailPointSnapshot>();
            Style = style ?? string.Empty;
            TrackClass = trackClass ?? string.Empty;
        }

        public string Id { get; }

        public string Name { get; }

        public string NodeAId { get; }

        public string NodeBId { get; }

        public float Ax { get; }

        public float Az { get; }

        public float Bx { get; }

        public float Bz { get; }

        public float Length { get; }

        public float SpeedLimit { get; }

        public string GroupId { get; }

        public bool IsAvailable { get; }

        public bool IsGroupEnabled { get; }

        public bool IsInvisible { get; }

        public System.Collections.Generic.IReadOnlyList<WebRailPointSnapshot> Points { get; }

        public string Style { get; }

        public string TrackClass { get; }
    }
}
