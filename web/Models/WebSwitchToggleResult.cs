using System;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebSwitchToggleResult
    {
        public WebSwitchToggleResult(
            string nodeId,
            bool requestedThrown,
            bool appliedThrown,
            bool succeeded,
            string reason,
            WebSwitchSnapshot snapshot,
            DateTimeOffset? capturedAtUtc = null)
        {
            NodeId = nodeId ?? string.Empty;
            RequestedThrown = requestedThrown;
            AppliedThrown = appliedThrown;
            Succeeded = succeeded;
            Reason = reason ?? string.Empty;
            Snapshot = snapshot;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public string NodeId { get; }

        public bool RequestedThrown { get; }

        public bool AppliedThrown { get; }

        public bool Succeeded { get; }

        public string Reason { get; }

        public WebSwitchSnapshot Snapshot { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
