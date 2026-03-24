using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveControlRequestedEvent
    {
        public WebLocomotiveControlRequestedEvent(string operationId, WebLocomotiveControlRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveControlRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveControlRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
