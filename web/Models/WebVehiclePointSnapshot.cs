namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehiclePointSnapshot
    {
        public WebVehiclePointSnapshot(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }
    }
}
