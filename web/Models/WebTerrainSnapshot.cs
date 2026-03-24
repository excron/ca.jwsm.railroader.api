namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTerrainSnapshot
    {
        public WebTerrainSnapshot(
            float minX,
            float maxX,
            float minZ,
            float maxZ,
            int width,
            int height,
            float minY,
            float maxY,
            byte[] normalizedHeights,
            byte[] coverageMask,
            byte[] vegetationMask,
            byte[] waterMask)
        {
            MinX = minX;
            MaxX = maxX;
            MinZ = minZ;
            MaxZ = maxZ;
            Width = width;
            Height = height;
            MinY = minY;
            MaxY = maxY;
            NormalizedHeights = normalizedHeights ?? System.Array.Empty<byte>();
            CoverageMask = coverageMask ?? System.Array.Empty<byte>();
            VegetationMask = vegetationMask ?? System.Array.Empty<byte>();
            WaterMask = waterMask ?? System.Array.Empty<byte>();
        }

        public float MinX { get; }

        public float MaxX { get; }

        public float MinZ { get; }

        public float MaxZ { get; }

        public int Width { get; }

        public int Height { get; }

        public float MinY { get; }

        public float MaxY { get; }

        public byte[] NormalizedHeights { get; }

        public byte[] CoverageMask { get; }

        public byte[] VegetationMask { get; }

        public byte[] WaterMask { get; }
    }
}
