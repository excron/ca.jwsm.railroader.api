using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public readonly struct CouplerEndId : IEquatable<CouplerEndId>
    {
        public CouplerEndId(VehicleId vehicleId, string end)
        {
            VehicleId = vehicleId;
            End = end ?? string.Empty;
        }

        public VehicleId VehicleId { get; }

        public string End { get; }

        public bool Equals(CouplerEndId other)
        {
            return VehicleId.Equals(other.VehicleId)
                && string.Equals(End, other.End, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is CouplerEndId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (VehicleId.GetHashCode() * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(End ?? string.Empty);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", VehicleId, End);
        }
    }
}
