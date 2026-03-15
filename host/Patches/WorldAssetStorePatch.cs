using AssetPack.Runtime;
using Ca.Jwsm.Railroader.Api.Host.Services;
using HarmonyLib;
using Model.Database;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    internal static class WorldAssetStoreState
    {
        internal static WorldAssetStoreService Service;
    }

    [HarmonyPatch(typeof(PrefabStore), "Create")]
    internal static class PrefabStoreCreatePatch
    {
        private static void Postfix(PrefabStore __result)
        {
            WorldAssetStoreState.Service?.RegisterStores(__result);
        }
    }

    [HarmonyPatch(typeof(AssetPackRuntimeStore), "get_BasePath")]
    internal static class AssetPackRuntimeStoreBasePathPatch
    {
        private static bool Prefix(AssetPackRuntimeStore __instance, ref string __result)
        {
            if (WorldAssetStoreState.Service == null ||
                !WorldAssetStoreState.Service.TryResolveBasePath(__instance != null ? __instance.Identifier : null, out var basePath))
            {
                return true;
            }

            __result = basePath;
            return false;
        }
    }
}
