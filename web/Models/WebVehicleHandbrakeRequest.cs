namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehicleHandbrakeRequest
    {
        public string VehicleId { get; set; } = string.Empty;

        public bool Apply { get; set; }
    }
}
