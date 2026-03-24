namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveModeRequest
    {
        public string VehicleId { get; set; } = string.Empty;

        public WebLocomotiveControlMode Mode { get; set; } = WebLocomotiveControlMode.Unknown;
    }
}
