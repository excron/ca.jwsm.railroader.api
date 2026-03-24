namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCompiledLabelManifest
    {
        public WebCompiledLabelManifest(
            string deliveryMode,
            int count,
            int majorCount,
            int secondaryCount)
        {
            DeliveryMode = string.IsNullOrWhiteSpace(deliveryMode) ? "inline" : deliveryMode;
            Count = count;
            MajorCount = majorCount;
            SecondaryCount = secondaryCount;
        }

        public string DeliveryMode { get; }

        public int Count { get; }

        public int MajorCount { get; }

        public int SecondaryCount { get; }
    }
}
