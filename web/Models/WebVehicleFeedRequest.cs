namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehicleFeedRequest
    {
        public float? MinX { get; set; }

        public float? MaxX { get; set; }

        public float? MinZ { get; set; }

        public float? MaxZ { get; set; }

        public float PaddingMeters { get; set; } = 150f;

        public string FocusVehicleId { get; set; } = string.Empty;
    }
}
