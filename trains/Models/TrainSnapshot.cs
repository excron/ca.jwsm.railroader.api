using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class TrainSnapshot
    {
        public TrainSnapshot(TrainId id, string displayName, IReadOnlyList<VehicleSnapshot> vehicles)
        {
            Id = id;
            DisplayName = displayName;
            Vehicles = vehicles ?? new VehicleSnapshot[0];
        }

        public TrainId Id { get; }

        public string DisplayName { get; }

        public IReadOnlyList<VehicleSnapshot> Vehicles { get; }
    }
}
