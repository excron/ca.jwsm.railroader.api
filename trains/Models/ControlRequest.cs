using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class ControlRequest
    {
        public ControlRequest(
            VehicleId vehicleId,
            LocomotiveControlKind controlKind,
            float requestedValue,
            string source = null,
            DateTimeOffset? timestamp = null)
        {
            VehicleId = vehicleId;
            ControlKind = controlKind;
            RequestedValue = requestedValue;
            Source = source;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public LocomotiveControlKind ControlKind { get; }

        public float RequestedValue { get; }

        public string Source { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
