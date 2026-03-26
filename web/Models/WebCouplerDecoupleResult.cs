using System;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCouplerDecoupleResult
    {
        public WebCouplerDecoupleResult(
            string couplerId,
            string vehicleId,
            string logicalEnd,
            string connectedVehicleId,
            bool succeeded,
            string reason,
            DateTimeOffset? capturedAtUtc = null)
        {
            CouplerId = couplerId ?? string.Empty;
            VehicleId = vehicleId ?? string.Empty;
            LogicalEnd = logicalEnd ?? string.Empty;
            ConnectedVehicleId = connectedVehicleId ?? string.Empty;
            Succeeded = succeeded;
            Reason = reason ?? string.Empty;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public string CouplerId { get; }

        public string VehicleId { get; }

        public string LogicalEnd { get; }

        public string ConnectedVehicleId { get; }

        public bool Succeeded { get; }

        public string Reason { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
