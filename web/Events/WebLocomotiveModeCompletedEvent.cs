using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveModeCompletedEvent
    {
        public WebLocomotiveModeCompletedEvent(
            string operationId,
            WebLocomotiveModeRequest request,
            bool succeeded,
            string errorMessage,
            WebLocomotiveModeResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveModeRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveModeRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebLocomotiveModeResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
