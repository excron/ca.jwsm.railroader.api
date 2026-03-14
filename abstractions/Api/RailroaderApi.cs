using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Api
{
    public static class RailroaderApi
    {
        private static readonly object Sync = new object();
        private static IApiHost _current;

        public static IApiHost Current
        {
            get
            {
                lock (Sync)
                {
                    return _current;
                }
            }
        }

        public static bool IsAvailable
        {
            get
            {
                lock (Sync)
                {
                    return _current != null;
                }
            }
        }

        public static bool TryGet(out IApiHost host)
        {
            lock (Sync)
            {
                host = _current;
                return host != null;
            }
        }

        public static void Attach(IApiHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            lock (Sync)
            {
                _current = host;
            }
        }

        public static void Detach(IApiHost host)
        {
            lock (Sync)
            {
                if (ReferenceEquals(_current, host))
                {
                    _current = null;
                }
            }
        }
    }
}
