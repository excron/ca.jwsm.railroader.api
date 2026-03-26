using System;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehicleHandbrakeResult
    {
        public WebVehicleHandbrakeResult(
            VehicleId vehicleId,
            bool requestedApplied,
            bool applied,
            string reason,
            DateTimeOffset? capturedAtUtc = null)
        {
            VehicleId = vehicleId;
            RequestedApplied = requestedApplied;
            Applied = applied;
            Reason = reason ?? string.Empty;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public bool RequestedApplied { get; }

        public bool Applied { get; }

        public string Reason { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
