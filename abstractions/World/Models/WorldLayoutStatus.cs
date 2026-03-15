namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Models
{
    public sealed class WorldLayoutStatus
    {
        public WorldLayoutStatus(bool hasSource, string sourceId, int revision, string summary)
        {
            HasSource = hasSource;
            SourceId = sourceId ?? string.Empty;
            Revision = revision;
            Summary = summary ?? string.Empty;
        }

        public bool HasSource { get; }

        public string SourceId { get; }

        public int Revision { get; }

        public string Summary { get; }
    }
}
