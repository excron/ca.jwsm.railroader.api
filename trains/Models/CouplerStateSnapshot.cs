namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerStateSnapshot
    {
        public CouplerStateSnapshot(CouplerEndId couplerEndId, bool isCoupled, VehicleId connectedVehicleId)
        {
            CouplerEndId = couplerEndId;
            IsCoupled = isCoupled;
            ConnectedVehicleId = connectedVehicleId;
        }

        public CouplerEndId CouplerEndId { get; }

        public bool IsCoupled { get; }

        public VehicleId ConnectedVehicleId { get; }
    }
}
