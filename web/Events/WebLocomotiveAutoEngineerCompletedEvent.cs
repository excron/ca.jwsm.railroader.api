using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebLocomotiveAutoEngineerCompletedEvent
    {
        public WebLocomotiveAutoEngineerCompletedEvent(
            string operationId,
            WebLocomotiveAutoEngineerRequest request,
            bool succeeded,
            string errorMessage,
            WebLocomotiveAutoEngineerResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebLocomotiveAutoEngineerRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebLocomotiveAutoEngineerRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebLocomotiveAutoEngineerResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
