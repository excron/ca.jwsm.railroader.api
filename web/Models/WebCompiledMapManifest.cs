using System;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCompiledMapManifest
    {
        public WebCompiledMapManifest(
            string graphSignature,
            int compilerVersion,
            DateTimeOffset compiledAtUtc,
            WebCompiledTrackManifest track,
            WebCompiledTerrainManifest terrain,
            WebCompiledLabelManifest labels)
        {
            GraphSignature = graphSignature ?? string.Empty;
            CompilerVersion = compilerVersion;
            CompiledAtUtc = compiledAtUtc;
            Track = track;
            Terrain = terrain;
            Labels = labels;
        }

        public string GraphSignature { get; }

        public int CompilerVersion { get; }

        public DateTimeOffset CompiledAtUtc { get; }

        public WebCompiledTrackManifest Track { get; }

        public WebCompiledTerrainManifest Terrain { get; }

        public WebCompiledLabelManifest Labels { get; }
    }
}
