namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebSwitchSnapshot
    {
        public WebSwitchSnapshot(
            string nodeId,
            string displayName,
            float x,
            float y,
            float z,
            string enterSegmentId,
            string normalSegmentId,
            string reversedSegmentId,
            string activeSegmentId,
            bool isThrown,
            bool displayThrown,
            bool isCtcSwitch,
            bool isUnlocked,
            int connectedSegmentCount)
        {
            NodeId = nodeId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            X = x;
            Y = y;
            Z = z;
            EnterSegmentId = enterSegmentId ?? string.Empty;
            NormalSegmentId = normalSegmentId ?? string.Empty;
            ReversedSegmentId = reversedSegmentId ?? string.Empty;
            ActiveSegmentId = activeSegmentId ?? string.Empty;
            IsThrown = isThrown;
            DisplayThrown = displayThrown;
            IsCtcSwitch = isCtcSwitch;
            IsUnlocked = isUnlocked;
            ConnectedSegmentCount = connectedSegmentCount;
        }

        public string NodeId { get; }

        public string DisplayName { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public string EnterSegmentId { get; }

        public string NormalSegmentId { get; }

        public string ReversedSegmentId { get; }

        public string ActiveSegmentId { get; }

        public bool IsThrown { get; }

        public bool DisplayThrown { get; }

        public bool IsCtcSwitch { get; }

        public bool IsUnlocked { get; }

        public int ConnectedSegmentCount { get; }
    }
}
