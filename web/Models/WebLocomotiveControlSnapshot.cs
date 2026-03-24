using System;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebLocomotiveControlSnapshot
    {
        public WebLocomotiveControlSnapshot(
            VehicleId vehicleId,
            string displayName,
            string prettyId,
            WebLocomotiveControlMode mode,
            bool isDiesel,
            int throttleValueSteps,
            int throttleDisplay,
            float throttle,
            float reverser,
            float independentBrake,
            float trainBrake,
            float speedMph,
            float brakePipePressurePsi,
            float brakeCylinderPressurePsi,
            DateTimeOffset? capturedAtUtc = null)
        {
            VehicleId = vehicleId;
            DisplayName = displayName ?? string.Empty;
            PrettyId = prettyId ?? string.Empty;
            Mode = mode;
            IsDiesel = isDiesel;
            ThrottleValueSteps = throttleValueSteps;
            ThrottleDisplay = throttleDisplay;
            Throttle = throttle;
            Reverser = reverser;
            IndependentBrake = independentBrake;
            TrainBrake = trainBrake;
            SpeedMph = speedMph;
            BrakePipePressurePsi = brakePipePressurePsi;
            BrakeCylinderPressurePsi = brakeCylinderPressurePsi;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public VehicleId VehicleId { get; }

        public string DisplayName { get; }

        public string PrettyId { get; }

        public WebLocomotiveControlMode Mode { get; }

        public bool IsDiesel { get; }

        public int ThrottleValueSteps { get; }

        public int ThrottleDisplay { get; }

        public float Throttle { get; }

        public float Reverser { get; }

        public float IndependentBrake { get; }

        public float TrainBrake { get; }

        public float SpeedMph { get; }

        public float BrakePipePressurePsi { get; }

        public float BrakeCylinderPressurePsi { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
