using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using Ca.Jwsm.Railroader.Api.Host.Bootstrap;
using UnityModManagerNet;

namespace Ca.Jwsm.Railroader.Api.Host
{
    public static class Main
    {
        private static IApiHost _current;
        private static HostBootstrapper _bootstrapper;

        public static IApiHost Current
        {
            get { return _current; }
        }

        public static IApiHost Initialize()
        {
            if (_current != null)
            {
                return _current;
            }

            _bootstrapper = new HostBootstrapper();
            _current = _bootstrapper.Bootstrap();
            RailroaderApi.Attach(_current);
            return _current;
        }

        public static bool Load(UnityModManager.ModEntry entry)
        {
            var host = Initialize();
            entry.OnUpdate = (_, __) =>
            {
                if (host.Services.TryGet<IWorldLayoutService>(out var worldLayout))
                {
                    worldLayout.Tick();
                }
            };
            entry.OnUnload = _ =>
            {
                Shutdown();
                return true;
            };
            return true;
        }

        private static void Shutdown()
        {
            if (_current == null)
            {
                return;
            }

            RailroaderApi.Detach(_current);

            try
            {
                _bootstrapper?.Shutdown();
            }
            catch
            {
            }

            _bootstrapper = null;
            _current = null;
        }
    }
}
