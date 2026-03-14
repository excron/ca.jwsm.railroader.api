using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using Game.State;
using HarmonyLib;
using Model;
using Model.Physics;
using UI;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    [HarmonyPatch]
    internal static class EquipmentPurchaseSpawnReasonPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("Game.State.EquipmentPurchase");
            return type == null ? null : AccessTools.Method(type, "HandleRequest");
        }

        [HarmonyPrefix]
        private static void Prefix(out IDisposable __state)
        {
            __state = TrainIntegrationState.Service == null
                ? null
                : TrainIntegrationState.Service.PushSpawnReason(VehicleSpawnReason.PurchasedNew);
        }

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception, IDisposable __state)
        {
            __state?.Dispose();
            return __exception;
        }
    }

    [HarmonyPatch(typeof(TrainController), "CreateCarIfNeeded")]
    internal static class TrainControllerCreateCarIfNeededPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Car __result)
        {
            TrainIntegrationState.Service?.PublishVehicleAdded(__result);
        }
    }

    [HarmonyPatch(typeof(TrainController), "WillRemoveCar")]
    internal static class TrainControllerWillRemoveCarPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Car car)
        {
            TrainIntegrationState.Service?.PublishVehicleRemoved(car);
        }
    }

    [HarmonyPatch(typeof(TrainController), nameof(TrainController.IntegrationSetDidCouple))]
    internal static class TrainControllerIntegrationSetDidCouplePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Car car0, Car car1, float deltaVelocity, out bool __state)
        {
            __state = TrainIntegrationState.Service == null
                || TrainIntegrationState.Service.PublishCouplerAttempt(car0, car1, deltaVelocity);
            return __state;
        }

        [HarmonyPostfix]
        private static void Postfix(Car car0, Car car1, float deltaVelocity, bool __state)
        {
            if (__state)
            {
                TrainIntegrationState.Service?.PublishCoupled(car0, car1, deltaVelocity);
            }
        }
    }

    [HarmonyPatch(typeof(TrainBrakeDisplay), "Awake")]
    internal static class TrainBrakeDisplayPatch
    {
        [HarmonyPostfix]
        private static void Postfix(TrainBrakeDisplay __instance)
        {
            TrainIntegrationState.Service?.PublishTrainBrakeDisplayAvailable(__instance);
        }
    }

    [HarmonyPatch]
    internal static class IntegrationSetIntegrateConstraintsPatch
    {
        private sealed class IterationState
        {
            public float[] PositionsBefore;
            public float[] RadiiBefore;
            public float[] AccumulatedDeltaSeparation;
            public int IterationsThisTick;
            public int LastAccumulatedFrame = -1;
        }

        private static readonly ConditionalWeakTable<IntegrationSet, IterationState> States = new ConditionalWeakTable<IntegrationSet, IterationState>();
        private static readonly Dictionary<Type, FieldInfo> ElementsFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly Dictionary<string, FieldInfo> FieldCache = new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<string, PropertyInfo> PropertyCache = new Dictionary<string, PropertyInfo>();

        static IEnumerable<MethodBase> TargetMethods()
        {
            var method = AccessTools.Method(typeof(IntegrationSet), "IntegrateConstraints", new[] { typeof(float) });
            if (method != null)
            {
                yield return method;
            }
        }

        [HarmonyPrefix]
        private static void Prefix(IntegrationSet __instance)
        {
            try
            {
                var elements = GetElementsArray(__instance);
                if (elements == null || elements.Length < 2)
                {
                    return;
                }

                int count = elements.Length;
                var state = States.GetOrCreateValue(__instance);
                if (state.LastAccumulatedFrame != UnityEngine.Time.frameCount)
                {
                    state.LastAccumulatedFrame = UnityEngine.Time.frameCount;
                    state.IterationsThisTick = 0;
                    EnsureArray(ref state.AccumulatedDeltaSeparation, count - 1);
                    Array.Clear(state.AccumulatedDeltaSeparation, 0, count - 1);
                }

                EnsureArray(ref state.PositionsBefore, count);
                EnsureArray(ref state.RadiiBefore, count);

                for (int i = 0; i < count; i++)
                {
                    object element = elements.GetValue(i);
                    state.PositionsBefore[i] = GetFloat(element, "position");
                    float radius = GetFloat(element, "CarRadius");
                    state.RadiiBefore[i] = Math.Abs(radius) > 1e-6f ? radius : GetFloat(element, "radius");
                }
            }
            catch
            {
            }
        }

        [HarmonyPostfix]
        private static void Postfix(IntegrationSet __instance, float wholeDeltaTime)
        {
            try
            {
                if (wholeDeltaTime <= 1e-6f)
                {
                    return;
                }

                var elements = GetElementsArray(__instance);
                if (elements == null || elements.Length < 2)
                {
                    return;
                }

                IterationState state;
                if (!States.TryGetValue(__instance, out state)
                    || state.PositionsBefore == null
                    || state.RadiiBefore == null
                    || state.AccumulatedDeltaSeparation == null)
                {
                    return;
                }

                int linkCount = elements.Length - 1;
                for (int i = 0; i < linkCount; i++)
                {
                    float separationBefore = state.PositionsBefore[i + 1] - state.RadiiBefore[i + 1]
                        - (state.PositionsBefore[i] + state.RadiiBefore[i]);

                    object firstElement = elements.GetValue(i);
                    object secondElement = elements.GetValue(i + 1);

                    float firstPosition = GetFloat(firstElement, "position");
                    float firstRadius = GetFloat(firstElement, "CarRadius");
                    if (Math.Abs(firstRadius) < 1e-6f)
                    {
                        firstRadius = GetFloat(firstElement, "radius");
                    }

                    float secondPosition = GetFloat(secondElement, "position");
                    float secondRadius = GetFloat(secondElement, "CarRadius");
                    if (Math.Abs(secondRadius) < 1e-6f)
                    {
                        secondRadius = GetFloat(secondElement, "radius");
                    }

                    float separationAfter = secondPosition - secondRadius - (firstPosition + firstRadius);
                    state.AccumulatedDeltaSeparation[i] += separationAfter - separationBefore;
                }

                state.IterationsThisTick++;
                if (state.IterationsThisTick >= 4)
                {
                    TrainIntegrationState.Service?.PublishConstraintTelemetry(__instance, wholeDeltaTime, state.AccumulatedDeltaSeparation);
                }
            }
            catch
            {
            }
        }

        private static Array GetElementsArray(IntegrationSet set)
        {
            var type = set.GetType();
            FieldInfo fieldInfo;
            if (!ElementsFieldCache.TryGetValue(type, out fieldInfo))
            {
                fieldInfo = AccessTools.Field(type, "elements") ?? AccessTools.Field(type, "_elements");
                ElementsFieldCache[type] = fieldInfo;
            }

            return fieldInfo == null ? null : fieldInfo.GetValue(set) as Array;
        }

        private static void EnsureArray(ref float[] buffer, int length)
        {
            if (buffer == null || buffer.Length != length)
            {
                buffer = new float[length];
            }
        }

        private static float GetFloat(object target, string name)
        {
            if (target == null)
            {
                return 0f;
            }

            var type = target.GetType();
            string key = type.FullName + "|" + name;

            FieldInfo fieldInfo;
            if (!FieldCache.TryGetValue(key, out fieldInfo))
            {
                fieldInfo = AccessTools.Field(type, name);
                FieldCache[key] = fieldInfo;
            }

            if (fieldInfo != null && fieldInfo.GetValue(target) is float fieldValue)
            {
                return fieldValue;
            }

            PropertyInfo propertyInfo;
            if (!PropertyCache.TryGetValue(key, out propertyInfo))
            {
                propertyInfo = AccessTools.Property(type, name);
                PropertyCache[key] = propertyInfo;
            }

            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(target, null);
                if (value is float floatValue)
                {
                    return floatValue;
                }

                if (value is double doubleValue)
                {
                    return (float)doubleValue;
                }
            }

            return 0f;
        }
    }

    internal static class TrainIntegrationState
    {
        internal static TrainIntegrationService Service { get; set; }
    }
}
