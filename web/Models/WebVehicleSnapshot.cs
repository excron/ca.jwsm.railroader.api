using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehicleSnapshot
    {
        public WebVehicleSnapshot(
            VehicleId id,
            string displayName,
            string prettyId,
            string definitionIdentifier,
            string archetype,
            string consistId,
            string consistName,
            int consistVehicleCount,
            string areaName,
            string areaColorHex,
            string destinationName,
            string destinationAreaName,
            string destinationAreaColorHex,
            bool isAtDestination,
            string segmentId,
            float distance,
            int end,
            float lengthMeters,
            float x,
            float y,
            float z,
            System.Collections.Generic.IReadOnlyList<WebVehiclePointSnapshot> bodyPoints,
            bool isLocomotive,
            float velocity,
            bool handbrakeApplied)
        {
            Id = id;
            DisplayName = displayName ?? string.Empty;
            PrettyId = prettyId ?? string.Empty;
            DefinitionIdentifier = definitionIdentifier ?? string.Empty;
            Archetype = archetype ?? string.Empty;
            ConsistId = consistId ?? string.Empty;
            ConsistName = consistName ?? string.Empty;
            ConsistVehicleCount = consistVehicleCount;
            AreaName = areaName ?? string.Empty;
            AreaColorHex = areaColorHex ?? string.Empty;
            DestinationName = destinationName ?? string.Empty;
            DestinationAreaName = destinationAreaName ?? string.Empty;
            DestinationAreaColorHex = destinationAreaColorHex ?? string.Empty;
            IsAtDestination = isAtDestination;
            SegmentId = segmentId ?? string.Empty;
            Distance = distance;
            End = end;
            LengthMeters = lengthMeters;
            X = x;
            Y = y;
            Z = z;
            BodyPoints = bodyPoints ?? System.Array.Empty<WebVehiclePointSnapshot>();
            IsLocomotive = isLocomotive;
            Velocity = velocity;
            HandbrakeApplied = handbrakeApplied;
        }

        public VehicleId Id { get; }

        public string DisplayName { get; }

        public string PrettyId { get; }

        public string DefinitionIdentifier { get; }

        public string Archetype { get; }

        public string ConsistId { get; }

        public string ConsistName { get; }

        public int ConsistVehicleCount { get; }

        public string AreaName { get; }

        public string AreaColorHex { get; }

        public string DestinationName { get; }

        public string DestinationAreaName { get; }

        public string DestinationAreaColorHex { get; }

        public bool IsAtDestination { get; }

        public string SegmentId { get; }

        public float Distance { get; }

        public int End { get; }

        public float LengthMeters { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public System.Collections.Generic.IReadOnlyList<WebVehiclePointSnapshot> BodyPoints { get; }

        public bool IsLocomotive { get; }

        public float Velocity { get; }

        public bool HandbrakeApplied { get; }
    }
}
