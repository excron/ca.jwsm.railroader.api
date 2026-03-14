using System;

namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public readonly struct UiAnchorId : IEquatable<UiAnchorId>
    {
        public static readonly UiAnchorId None = new UiAnchorId("none");

        public UiAnchorId(string value)
        {
            Value = value ?? string.Empty;
        }

        public string Value { get; }

        public bool Equals(UiAnchorId other)
        {
            return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is UiAnchorId other && Equals(other);
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
