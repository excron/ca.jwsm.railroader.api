using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Models
{
    public sealed class WorldLayoutDocument
    {
        public WorldLayoutDocument(string sourcePath, string json)
        {
            SourcePath = sourcePath ?? string.Empty;
            Json = json ?? string.Empty;
        }

        public string SourcePath { get; }

        public string Json { get; }
    }
}
