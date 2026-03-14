using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Common
{
    public readonly struct CapabilityId : IEquatable<CapabilityId>
    {
        public CapabilityId(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool Equals(CapabilityId other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is CapabilityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Value ?? string.Empty);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(CapabilityId left, CapabilityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CapabilityId left, CapabilityId right)
        {
            return !left.Equals(right);
        }
    }
}
