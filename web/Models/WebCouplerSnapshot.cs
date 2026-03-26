namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebCouplerSnapshot
    {
        public WebCouplerSnapshot(
            string couplerId,
            string vehicleId,
            string vehiclePrettyId,
            string logicalEnd,
            string connectedVehicleId,
            string connectedVehiclePrettyId,
            string connectedLogicalEnd,
            float x,
            float y,
            float z,
            float ax,
            float ay,
            float az,
            float bx,
            float by,
            float bz)
        {
            CouplerId = couplerId ?? string.Empty;
            VehicleId = vehicleId ?? string.Empty;
            VehiclePrettyId = vehiclePrettyId ?? string.Empty;
            LogicalEnd = logicalEnd ?? string.Empty;
            ConnectedVehicleId = connectedVehicleId ?? string.Empty;
            ConnectedVehiclePrettyId = connectedVehiclePrettyId ?? string.Empty;
            ConnectedLogicalEnd = connectedLogicalEnd ?? string.Empty;
            X = x;
            Y = y;
            Z = z;
            Ax = ax;
            Ay = ay;
            Az = az;
            Bx = bx;
            By = by;
            Bz = bz;
        }

        public string CouplerId { get; }

        public string VehicleId { get; }

        public string VehiclePrettyId { get; }

        public string LogicalEnd { get; }

        public string ConnectedVehicleId { get; }

        public string ConnectedVehiclePrettyId { get; }

        public string ConnectedLogicalEnd { get; }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        public float Ax { get; }

        public float Ay { get; }

        public float Az { get; }

        public float Bx { get; }

        public float By { get; }

        public float Bz { get; }
    }
}
