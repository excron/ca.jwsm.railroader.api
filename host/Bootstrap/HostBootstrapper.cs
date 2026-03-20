using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using HarmonyLib;

namespace Ca.Jwsm.Railroader.Api.Host.Bootstrap
{
    public sealed class HostBootstrapper
    {
        public const string HarmonyId = "ca.jwsm.railroader.api.host";

        private readonly Harmony _harmony = new Harmony(HarmonyId);

        public IApiHost Bootstrap()
        {
            var host = new HostCompositionRoot().Build();
            _harmony.PatchAll(typeof(HostBootstrapper).Assembly);
            return host;
        }

        public void Shutdown()
        {
            HostPatchState.Reset();
            _harmony.UnpatchAll(HarmonyId);
        }
    }
}
