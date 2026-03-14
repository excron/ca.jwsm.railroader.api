namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class ConsistSnapshot
    {
        public ConsistSnapshot(TrainId trainId, int vehicleCount)
        {
            TrainId = trainId;
            VehicleCount = vehicleCount;
        }

        public TrainId TrainId { get; }

        public int VehicleCount { get; }
    }
}
