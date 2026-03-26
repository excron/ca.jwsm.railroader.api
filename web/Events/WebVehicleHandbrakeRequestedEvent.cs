using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebVehicleHandbrakeRequestedEvent
    {
        public WebVehicleHandbrakeRequestedEvent(string operationId, WebVehicleHandbrakeRequest request, DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebVehicleHandbrakeRequest();
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebVehicleHandbrakeRequest Request { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
