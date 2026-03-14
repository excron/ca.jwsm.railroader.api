using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class VehicleRemovedEvent
    {
        public VehicleRemovedEvent(VehicleId vehicleId, string displayName)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? string.Empty;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }
    }
}
