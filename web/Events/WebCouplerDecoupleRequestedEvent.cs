using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebCouplerDecoupleRequestedEvent
    {
        public WebCouplerDecoupleRequestedEvent(string operationId, WebCouplerDecoupleRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebCouplerDecoupleRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebCouplerDecoupleRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
