using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebWaterFeatureSnapshot
    {
        public WebWaterFeatureSnapshot(
            string id,
            string kind,
            float widthMeters,
            IReadOnlyList<WebRailPointSnapshot> points,
            IReadOnlyList<WebRailPointSnapshot> centerPoints)
        {
            Id = id ?? string.Empty;
            Kind = kind ?? string.Empty;
            WidthMeters = widthMeters;
            Points = points ?? Array.Empty<WebRailPointSnapshot>();
            CenterPoints = centerPoints ?? Array.Empty<WebRailPointSnapshot>();
        }

        public string Id { get; }

        public string Kind { get; }

        public float WidthMeters { get; }

        public IReadOnlyList<WebRailPointSnapshot> Points { get; }

        public IReadOnlyList<WebRailPointSnapshot> CenterPoints { get; }
    }
}
