using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using Ca.Jwsm.Railroader.Api.Host.Bootstrap;
using UnityModManagerNet;

namespace Ca.Jwsm.Railroader.Api.Host
{
    public static class Main
    {
        private static IApiHost _current;

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

            _current = new HostBootstrapper().Bootstrap();
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
            return true;
        }
    }
}
