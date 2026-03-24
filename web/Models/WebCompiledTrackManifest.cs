using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCompiledTrackManifest
    {
        public WebCompiledTrackManifest(
            string deliveryMode,
            int segmentCount,
            int samplePointCount,
            IReadOnlyList<string> availableLods)
        {
            DeliveryMode = string.IsNullOrWhiteSpace(deliveryMode) ? "inline" : deliveryMode;
            SegmentCount = segmentCount;
            SamplePointCount = samplePointCount;
            AvailableLods = availableLods ?? Array.Empty<string>();
        }

        public string DeliveryMode { get; }

        public int SegmentCount { get; }

        public int SamplePointCount { get; }

        public IReadOnlyList<string> AvailableLods { get; }
    }
}
