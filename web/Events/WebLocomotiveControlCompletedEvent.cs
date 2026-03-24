using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveControlCompletedEvent
    {
        public WebLocomotiveControlCompletedEvent(
            string operationId,
            WebLocomotiveControlRequest request,
            bool succeeded,
            string errorMessage,
            WebLocomotiveControlResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveControlRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveControlRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebLocomotiveControlResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
