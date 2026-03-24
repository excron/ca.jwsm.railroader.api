using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebSwitchToggleCompletedEvent
    {
        public WebSwitchToggleCompletedEvent(
            string operationId,
            WebSwitchToggleRequest request,
            bool succeeded,
            string errorMessage,
            WebSwitchToggleResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebSwitchToggleRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebSwitchToggleRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebSwitchToggleResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
