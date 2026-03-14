using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public readonly struct TrainId : IEquatable<TrainId>
    {
        public TrainId(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool Equals(TrainId other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is TrainId other && Equals(other);
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
