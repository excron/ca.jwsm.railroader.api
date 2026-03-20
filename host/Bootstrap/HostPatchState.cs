using Ca.Jwsm.Railroader.Api.Host.Patches;

namespace Ca.Jwsm.Railroader.Api.Host.Bootstrap
{
    internal static class HostPatchState
    {
        internal static void Reset()
        {
            SaveLifecycle.Service = null;
            TrainIntegrationState.Service = null;
            CouplerInteractionState.Service = null;
            WorldLifecycleState.Service = null;
            WorldAssetStoreState.Service = null;
        }
    }
}
