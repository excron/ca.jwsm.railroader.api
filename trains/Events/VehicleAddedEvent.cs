using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class VehicleAddedEvent
    {
        public VehicleAddedEvent(VehicleId vehicleId, string displayName, VehicleSpawnReason spawnReason)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? string.Empty;
            SpawnReason = spawnReason;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }

        public VehicleSpawnReason SpawnReason { get; }
    }
}
