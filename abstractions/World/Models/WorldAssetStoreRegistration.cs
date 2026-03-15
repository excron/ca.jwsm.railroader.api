using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Models
{
    public sealed class WorldAssetStoreRegistration
    {
        public WorldAssetStoreRegistration(string identifier, string basePath)
        {
            Identifier = identifier ?? string.Empty;
            BasePath = basePath ?? string.Empty;
        }

        public string Identifier { get; }

        public string BasePath { get; }
    }
}
