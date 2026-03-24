using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebTrainPlacementRequestedEvent
    {
        public WebTrainPlacementRequestedEvent(string operationId, WebTrainPlacementRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebTrainPlacementRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebTrainPlacementRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
