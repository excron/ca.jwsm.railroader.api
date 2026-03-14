namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class VehicleSnapshot
    {
        public VehicleSnapshot(VehicleId id, string displayName, bool isLocomotive)
        {
            Id = id;
            DisplayName = displayName;
            IsLocomotive = isLocomotive;
        }

        public VehicleId Id { get; }

        public string DisplayName { get; }

        public bool IsLocomotive { get; }
    }
}
