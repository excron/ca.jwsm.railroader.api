using System;

namespace Ca.Jwsm.Railroader.Api.Persistence.Models
{
    public readonly struct ModDataKey : IEquatable<ModDataKey>
    {
        public ModDataKey(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool Equals(ModDataKey other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is ModDataKey other && Equals(other);
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
