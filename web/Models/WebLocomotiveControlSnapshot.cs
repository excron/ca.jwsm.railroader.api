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
            bool aeForward,
            int aeMaxSpeedMph,
            float? aeManualStopDistanceMeters,
            float aeTargetSpeedMph,
            string aePlannerStatus,
            string aeWaypointLocationString,
            string aeWaypointCoupleToCarId,
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
            AeForward = aeForward;
            AeMaxSpeedMph = aeMaxSpeedMph;
            AeManualStopDistanceMeters = aeManualStopDistanceMeters;
            AeTargetSpeedMph = aeTargetSpeedMph;
            AePlannerStatus = aePlannerStatus ?? string.Empty;
            AeWaypointLocationString = aeWaypointLocationString ?? string.Empty;
            AeWaypointCoupleToCarId = aeWaypointCoupleToCarId ?? string.Empty;
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

        public bool AeForward { get; }

        public int AeMaxSpeedMph { get; }

        public float? AeManualStopDistanceMeters { get; }

        public float AeTargetSpeedMph { get; }

        public string AePlannerStatus { get; }

        public string AeWaypointLocationString { get; }

        public string AeWaypointCoupleToCarId { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}
