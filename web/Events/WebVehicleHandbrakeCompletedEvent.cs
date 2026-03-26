using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebVehicleHandbrakeCompletedEvent
    {
        public WebVehicleHandbrakeCompletedEvent(
            string operationId,
            WebVehicleHandbrakeRequest request,
            bool succeeded,
            string errorMessage,
            WebVehicleHandbrakeResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebVehicleHandbrakeRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebVehicleHandbrakeRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebVehicleHandbrakeResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
