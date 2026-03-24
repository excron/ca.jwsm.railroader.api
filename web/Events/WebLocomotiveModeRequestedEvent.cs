using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveModeRequestedEvent
    {
        public WebLocomotiveModeRequestedEvent(string operationId, WebLocomotiveModeRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveModeRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveModeRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
