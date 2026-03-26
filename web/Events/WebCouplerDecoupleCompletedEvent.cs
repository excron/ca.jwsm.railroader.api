using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebCouplerDecoupleCompletedEvent
    {
        public WebCouplerDecoupleCompletedEvent(
            string operationId,
            WebCouplerDecoupleRequest request,
            bool succeeded,
            string errorMessage,
            WebCouplerDecoupleResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebCouplerDecoupleRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebCouplerDecoupleRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebCouplerDecoupleResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
