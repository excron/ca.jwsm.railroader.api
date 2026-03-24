using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebSwitchToggleRequestedEvent
    {
        public WebSwitchToggleRequestedEvent(string operationId, WebSwitchToggleRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebSwitchToggleRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebSwitchToggleRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
