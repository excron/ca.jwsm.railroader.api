using System;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveControlResult
    {
        public WebLocomotiveControlResult(
            VehicleId vehicleId,
            LocomotiveControlKind controlKind,
            float requestedValue,
            float acceptedValue,
            bool isBlocked,
            string reason,
            WebLocomotiveControlSnapshot snapshot,
            DateTimeOffset? capturedAtUtc = null)
        {
            VehicleId = vehicleId;
            ControlKind = controlKind;
            RequestedValue = requestedValue;
            AcceptedValue = acceptedValue;
            IsBlocked = isBlocked;
            Reason = reason ?? string.Empty;
            Snapshot = snapshot;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public LocomotiveControlKind ControlKind { get; }

        public float RequestedValue { get; }

        public float AcceptedValue { get; }

        public bool IsBlocked { get; }

        public string Reason { get; }

        public WebLocomotiveControlSnapshot Snapshot { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
