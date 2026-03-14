namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class SelectedLocomotiveContext
    {
        public SelectedLocomotiveContext(
            VehicleId selectedVehicleId,
            string selectedDisplayName = null,
            VehicleId authorityVehicleId = default(VehicleId),
            string authorityDisplayName = null,
            bool hasManualControls = false,
            bool hasSimplifiedControls = false)
        {
            SelectedVehicleId = selectedVehicleId;
            SelectedDisplayName = selectedDisplayName;
            AuthorityVehicleId = authorityVehicleId;
            AuthorityDisplayName = authorityDisplayName;
            HasManualControls = hasManualControls;
            HasSimplifiedControls = hasSimplifiedControls;
        }

        public VehicleId SelectedVehicleId { get; }

        public string SelectedDisplayName { get; }

        public VehicleId AuthorityVehicleId { get; }

        public string AuthorityDisplayName { get; }

        public bool HasManualControls { get; }

        public bool HasSimplifiedControls { get; }
    }
}
