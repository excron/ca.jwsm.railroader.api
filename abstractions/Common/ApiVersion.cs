using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Common
{
    public readonly struct ApiVersion : IEquatable<ApiVersion>
    {
        public ApiVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }

        public bool Equals(ApiVersion other)
        {
            return Major == other.Major && Minor == other.Minor && Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            return obj is ApiVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Major;
                hash = (hash * 397) ^ Minor;
                hash = (hash * 397) ^ Patch;
                return hash;
            }
        }

        public static bool operator ==(ApiVersion left, ApiVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ApiVersion left, ApiVersion right)
        {
            return !left.Equals(right);
        }
    }
}
