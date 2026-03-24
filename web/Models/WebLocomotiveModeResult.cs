using System;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveModeResult
    {
        public WebLocomotiveModeResult(
            VehicleId vehicleId,
            WebLocomotiveControlMode requestedMode,
            WebLocomotiveControlMode appliedMode,
            string reason,
            WebLocomotiveControlSnapshot snapshot,
            DateTimeOffset? capturedAtUtc = null)
        {
            VehicleId = vehicleId;
            RequestedMode = requestedMode;
            AppliedMode = appliedMode;
            Reason = reason ?? string.Empty;
            Snapshot = snapshot;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public WebLocomotiveControlMode RequestedMode { get; }

        public WebLocomotiveControlMode AppliedMode { get; }

        public string Reason { get; }

        public WebLocomotiveControlSnapshot Snapshot { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
