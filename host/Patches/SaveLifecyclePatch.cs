using System.Reflection;
using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Persistence.Models;
using Game.State;
using HarmonyLib;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    [HarmonyPatch(typeof(SaveManager))]
    internal static class SaveManagerLifecyclePatch
    {
        private static readonly FieldInfo SaveNameField = AccessTools.Field(typeof(SaveManager), "_saveName");

        [HarmonyPatch("Load")]
        [HarmonyPrefix]
        private static void LoadPrefix(string saveName)
        {
            SaveLifecycle.Publish(SaveLifecycleStage.Loading, ResolveSaveId(saveName, SaveManager.Shared));
        }

        [HarmonyPatch("Load")]
        [HarmonyPostfix]
        private static void LoadPostfix(string saveName)
        {
            SaveLifecycle.Publish(SaveLifecycleStage.Loaded, ResolveSaveId(saveName, SaveManager.Shared));
        }

        [HarmonyPatch("Save")]
        [HarmonyPrefix]
        private static void SavePrefix(SaveManager __instance, string saveName)
        {
            SaveLifecycle.Publish(SaveLifecycleStage.Saving, ResolveSaveId(saveName, __instance));
        }

        [HarmonyPatch("Save")]
        [HarmonyPostfix]
        private static void SavePostfix(SaveManager __instance, string saveName)
        {
            SaveLifecycle.Publish(SaveLifecycleStage.Saved, ResolveSaveId(saveName, __instance));
        }

        [HarmonyPatch(nameof(SaveManager.WillUnloadMap))]
        [HarmonyPrefix]
        private static void WillUnloadMapPrefix()
        {
            SaveLifecycle.Publish(SaveLifecycleStage.Unloading);
        }

        private static string ResolveSaveId(string saveName, SaveManager saveManager)
        {
            if (!string.IsNullOrWhiteSpace(saveName))
            {
                return saveName;
            }

            if (saveManager == null || SaveNameField == null)
            {
                return string.Empty;
            }

            return SaveNameField.GetValue(saveManager) as string ?? string.Empty;
        }
    }

    [HarmonyPatch(typeof(StateManager), "OnApplicationQuit")]
    internal static class StateManagerApplicationQuitPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            SaveLifecycle.Publish(SaveLifecycleStage.ApplicationQuitting);
        }
    }

    internal static class SaveLifecycle
    {
        internal static SaveContextService Service { get; set; }

        internal static void Publish(SaveLifecycleStage stage)
        {
            Service?.Publish(stage);
        }

        internal static void Publish(SaveLifecycleStage stage, string saveId)
        {
            Service?.Publish(stage, saveId);
        }
    }
}
