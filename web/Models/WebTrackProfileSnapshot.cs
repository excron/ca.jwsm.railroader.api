using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTrackProfileSnapshot
    {
        public WebTrackProfileSnapshot(
            string vehicleId,
            DateTimeOffset capturedAtUtc,
            float lookaheadMeters,
            bool forward,
            IReadOnlyList<string> consistVehicleIds,
            IReadOnlyList<WebTrackProfilePointSnapshot> points,
            IReadOnlyList<WebTrackProfileSignalSnapshot> signals)
        {
            VehicleId = vehicleId ?? string.Empty;
            CapturedAtUtc = capturedAtUtc;
            LookaheadMeters = lookaheadMeters;
            Forward = forward;
            ConsistVehicleIds = consistVehicleIds ?? Array.Empty<string>();
            Points = points ?? Array.Empty<WebTrackProfilePointSnapshot>();
            Signals = signals ?? Array.Empty<WebTrackProfileSignalSnapshot>();
        }

        public string VehicleId { get; }

        public DateTimeOffset CapturedAtUtc { get; }

        public float LookaheadMeters { get; }

        public bool Forward { get; }

        public IReadOnlyList<string> ConsistVehicleIds { get; }

        public IReadOnlyList<WebTrackProfilePointSnapshot> Points { get; }

        public IReadOnlyList<WebTrackProfileSignalSnapshot> Signals { get; }
    }
}
