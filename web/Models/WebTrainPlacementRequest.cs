namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTrainPlacementRequest
    {
        public string SegmentId { get; set; } = string.Empty;

        public float Distance { get; set; }

        public int End { get; set; }

        public string[] Identifiers { get; set; } = new string[0];
    }
}
