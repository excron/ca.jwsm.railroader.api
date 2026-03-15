using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;
using Game.Progression;
using HarmonyLib;
using Model.Ops;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    internal static class WorldLifecycleState
    {
        internal static IWorldLayoutService Service;
    }

    [HarmonyPatch(typeof(ProgressionManager), "Awake")]
    internal static class ProgressionManagerAwakePatch
    {
        private static void Prefix()
        {
            WorldLifecycleState.Service?.TryApplyEarly("ProgressionManager.Awake");
        }
    }

    [HarmonyPatch(typeof(MapFeatureManager), "Awake")]
    internal static class MapFeatureManagerAwakePatch
    {
        private static void Prefix()
        {
            WorldLifecycleState.Service?.TryApplyEarly("MapFeatureManager.Awake");
        }
    }

    [HarmonyPatch(typeof(OpsController), "Awake")]
    internal static class OpsControllerAwakePatch
    {
        private static void Prefix()
        {
            WorldLifecycleState.Service?.TryApplyEarly("OpsController.Awake");
        }
    }
}
