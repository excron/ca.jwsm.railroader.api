namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveAutoEngineerRequest
    {
        public string VehicleId { get; set; } = string.Empty;

        public WebLocomotiveAutoEngineerOperation Operation { get; set; } = WebLocomotiveAutoEngineerOperation.Unknown;

        public bool? Forward { get; set; }

        public int? MaxSpeedMph { get; set; }

        public float? DistanceMeters { get; set; }

        public string SegmentId { get; set; } = string.Empty;

        public float Distance { get; set; }

        public int End { get; set; }

        public string CoupleToCarId { get; set; } = string.Empty;
    }
}
