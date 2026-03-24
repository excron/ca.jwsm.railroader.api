using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveControlRequest
    {
        public string VehicleId { get; set; } = string.Empty;

        public LocomotiveControlKind ControlKind { get; set; } = LocomotiveControlKind.Unknown;

        public float Value { get; set; }
    }
}
