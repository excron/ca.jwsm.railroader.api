namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebTrackProfilePointSnapshot
    {
        public WebTrackProfilePointSnapshot(float distanceMeters, float gradePercent, float speedLimitMph, float x, float z)
        {
            DistanceMeters = distanceMeters;
            GradePercent = gradePercent;
            SpeedLimitMph = speedLimitMph;
            X = x;
            Z = z;
        }

        public float DistanceMeters { get; }

        public float GradePercent { get; }

        public float SpeedLimitMph { get; }

        public float X { get; }

        public float Z { get; }
    }
}
