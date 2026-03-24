namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCompiledTerrainManifest
    {
        public WebCompiledTerrainManifest(
            string deliveryMode,
            bool isAvailable,
            int width,
            int height,
            float minY,
            float maxY)
        {
            DeliveryMode = string.IsNullOrWhiteSpace(deliveryMode) ? "inline" : deliveryMode;
            IsAvailable = isAvailable;
            Width = width;
            Height = height;
            MinY = minY;
            MaxY = maxY;
        }

        public string DeliveryMode { get; }

        public bool IsAvailable { get; }

        public int Width { get; }

        public int Height { get; }

        public float MinY { get; }

        public float MaxY { get; }
    }
}
