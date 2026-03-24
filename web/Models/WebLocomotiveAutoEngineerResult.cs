using System;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveAutoEngineerResult
    {
        public WebLocomotiveAutoEngineerResult(
            VehicleId vehicleId,
            WebLocomotiveAutoEngineerOperation operation,
            string reason,
            WebLocomotiveControlSnapshot snapshot,
            DateTimeOffset? capturedAtUtc = null)
        {
            VehicleId = vehicleId;
            Operation = operation;
            Reason = reason ?? string.Empty;
            Snapshot = snapshot;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public WebLocomotiveAutoEngineerOperation Operation { get; }

        public string Reason { get; }

        public WebLocomotiveControlSnapshot Snapshot { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
