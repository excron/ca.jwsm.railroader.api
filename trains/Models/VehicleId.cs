using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public readonly struct VehicleId : IEquatable<VehicleId>
    {
        public VehicleId(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool Equals(VehicleId other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is VehicleId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }
    }
}
