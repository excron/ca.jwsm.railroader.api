using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Models
{
    public sealed class WorldLayoutSourceUpdate
    {
        public WorldLayoutSourceUpdate(string sourceId, int revision, IReadOnlyList<WorldLayoutDocument> documents)
        {
            SourceId = sourceId ?? string.Empty;
            Revision = revision;
            Documents = documents ?? Array.Empty<WorldLayoutDocument>();
        }

        public string SourceId { get; }

        public int Revision { get; }

        public IReadOnlyList<WorldLayoutDocument> Documents { get; }
    }
}
