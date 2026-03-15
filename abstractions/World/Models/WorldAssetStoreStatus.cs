namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Models
{
    public sealed class WorldAssetStoreStatus
    {
        public WorldAssetStoreStatus(int sourceCount, int storeCount, string summary)
        {
            SourceCount = sourceCount;
            StoreCount = storeCount;
            Summary = summary ?? string.Empty;
        }

        public int SourceCount { get; }

        public int StoreCount { get; }

        public string Summary { get; }
    }
}
