using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class VehicleRepairProgressedEvent
    {
        public VehicleRepairProgressedEvent(VehicleId vehicleId, string displayName, float conditionBefore, float conditionAfter, float repairUsed, object nativeVehicle)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? vehicleId.Value;
            ConditionBefore = conditionBefore;
            ConditionAfter = conditionAfter;
            RepairUsed = repairUsed;
            NativeVehicle = nativeVehicle;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }

        public float ConditionBefore { get; }

        public float ConditionAfter { get; }

        public float ConditionDelta => ConditionAfter - ConditionBefore;

        public float RepairUsed { get; }

        public object NativeVehicle { get; }
    }
}
