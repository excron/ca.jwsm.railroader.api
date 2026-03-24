using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebRailGraphSnapshot
    {
        public WebRailGraphSnapshot(
            DateTimeOffset capturedAtUtc,
            string graphSignature,
            DateTimeOffset? compiledAtUtc,
            WebCompiledMapManifest manifest,
            IReadOnlyList<string> groupIds,
            IReadOnlyList<WebRailNodeSnapshot> nodes,
            IReadOnlyList<WebRailSegmentSnapshot> segments,
            IReadOnlyList<WebMapLabelSnapshot> labels,
            WebTerrainSnapshot terrain)
        {
            CapturedAtUtc = capturedAtUtc;
            GraphSignature = graphSignature ?? string.Empty;
            CompiledAtUtc = compiledAtUtc;
            Manifest = manifest;
            GroupIds = groupIds ?? Array.Empty<string>();
            Nodes = nodes ?? Array.Empty<WebRailNodeSnapshot>();
            Segments = segments ?? Array.Empty<WebRailSegmentSnapshot>();
            Labels = labels ?? Array.Empty<WebMapLabelSnapshot>();
            Terrain = terrain;
        }

        public DateTimeOffset CapturedAtUtc { get; }

        public string GraphSignature { get; }

        public DateTimeOffset? CompiledAtUtc { get; }

        public WebCompiledMapManifest Manifest { get; }

        public IReadOnlyList<string> GroupIds { get; }

        public IReadOnlyList<WebRailNodeSnapshot> Nodes { get; }

        public IReadOnlyList<WebRailSegmentSnapshot> Segments { get; }

        public IReadOnlyList<WebMapLabelSnapshot> Labels { get; }

        public WebTerrainSnapshot Terrain { get; }
    }
}
