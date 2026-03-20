using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class VehicleRepairWorkAvailableEvent
    {
        public VehicleRepairWorkAvailableEvent(VehicleId vehicleId, string displayName, float repairWorkUnitsAvailable, object nativeVehicle)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? vehicleId.Value;
            RepairWorkUnitsAvailable = repairWorkUnitsAvailable;
            NativeVehicle = nativeVehicle;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }

        public float RepairWorkUnitsAvailable { get; }

        public object NativeVehicle { get; }
    }
}
