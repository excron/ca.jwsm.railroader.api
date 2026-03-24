using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveAutoEngineerRequestedEvent
    {
        public WebLocomotiveAutoEngineerRequestedEvent(string operationId, WebLocomotiveAutoEngineerRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveAutoEngineerRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveAutoEngineerRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
