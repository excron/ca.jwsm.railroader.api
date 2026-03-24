using System;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Events
{
    public sealed class WebTrainPlacementCompletedEvent
    {
        public WebTrainPlacementCompletedEvent(
            string operationId,
            WebTrainPlacementRequest request,
            bool succeeded,
            string errorMessage,
            WebTrainPlacementResult result,
            DateTimeOffset? timestamp = null)
        {
            OperationId = operationId ?? string.Empty;
            Request = request ?? new WebTrainPlacementRequest();
            Succeeded = succeeded;
            ErrorMessage = errorMessage ?? string.Empty;
            Result = result;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public string OperationId { get; }

        public WebTrainPlacementRequest Request { get; }

        public bool Succeeded { get; }

        public string ErrorMessage { get; }

        public WebTrainPlacementResult Result { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
