namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCouplerDecoupleRequest
    {
        public string VehicleId { get; set; } = string.Empty;

        public string LogicalEnd { get; set; } = string.Empty;
    }
}
