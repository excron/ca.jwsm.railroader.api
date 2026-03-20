using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class VehicleRepairWorkAvailableEvent
    {
        public VehicleRepairWorkAvailableEvent(VehicleId vehicleId, string displayName, RepairWorkEstimate repairEstimate, object nativeVehicle)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? vehicleId.Value;
            RepairEstimate = repairEstimate;
            NativeVehicle = nativeVehicle;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }

        public RepairWorkEstimate RepairEstimate { get; }

        public object NativeVehicle { get; }
    }
}
