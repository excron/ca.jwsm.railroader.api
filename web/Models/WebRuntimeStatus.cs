using System;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebRuntimeStatus
    {
        public WebRuntimeStatus(
            bool isHostAttached,
            bool isSaveLoaded,
            string saveId,
            bool isWorldReady,
            bool isGraphAvailable,
            bool isTrainControllerAvailable,
            long worldChangeSequence,
            long vehicleChangeSequence,
            int port,
            DateTimeOffset capturedAtUtc)
        {
            IsHostAttached = isHostAttached;
            IsSaveLoaded = isSaveLoaded;
            SaveId = saveId ?? string.Empty;
            IsWorldReady = isWorldReady;
            IsGraphAvailable = isGraphAvailable;
            IsTrainControllerAvailable = isTrainControllerAvailable;
            WorldChangeSequence = worldChangeSequence;
            VehicleChangeSequence = vehicleChangeSequence;
            Port = port;
            CapturedAtUtc = capturedAtUtc;
        }

        public bool IsHostAttached { get; }

        public bool IsSaveLoaded { get; }

        public string SaveId { get; }

        public bool IsWorldReady { get; }

        public bool IsGraphAvailable { get; }

        public bool IsTrainControllerAvailable { get; }

        public long WorldChangeSequence { get; }

        public long VehicleChangeSequence { get; }

        public int Port { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
