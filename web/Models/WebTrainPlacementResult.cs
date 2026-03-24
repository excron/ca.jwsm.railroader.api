namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTrainPlacementResult
    {
        public WebTrainPlacementResult(
            string operationId,
            string segmentId,
            float distance,
            int end,
            int requestedVehicleCount,
            int acceptedVehicleCount,
            string message)
        {
            OperationId = operationId ?? string.Empty;
            SegmentId = segmentId ?? string.Empty;
            Distance = distance;
            End = end;
            RequestedVehicleCount = requestedVehicleCount;
            AcceptedVehicleCount = acceptedVehicleCount;
            Message = message ?? string.Empty;
        }

        public string OperationId { get; }

        public string SegmentId { get; }

        public float Distance { get; }

        public int End { get; }

        public int RequestedVehicleCount { get; }

        public int AcceptedVehicleCount { get; }

        public string Message { get; }
    }
}
