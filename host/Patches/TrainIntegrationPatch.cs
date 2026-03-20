using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ca.Jwsm.Railroader.Api.Host.Diagnostics;
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

    [HarmonyPatch]
    internal static class PlaceConsistCommandSpawnReasonPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UI.Console.Commands.PlaceConsistCommand");
            return type == null ? null : AccessTools.Method(type, "Execute");
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

    [HarmonyPatch]
    internal static class ScriptWorldPlaceTrainSpawnReasonPatch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            var type = AccessTools.TypeByName("Game.Scripting.ScriptWorld");
            if (type == null)
            {
                yield break;
            }

            var placeTrain = AccessTools.Method(type, "place_train");
            if (placeTrain != null)
            {
                yield return placeTrain;
            }

            var placeTrainAtInterchange = AccessTools.Method(type, "place_train_at_interchange");
            if (placeTrainAtInterchange != null)
            {
                yield return placeTrainAtInterchange;
            }
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

    [HarmonyPatch]
    internal static class DefinitionEditorModeSpawnReasonPatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("UI.CarEditor.DefinitionEditorModeController");
            return type == null ? null : AccessTools.Method(type, "EditItemCar");
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
        private static void Postfix(Car __result, string carId)
        {
            try
            {
                TrainIntegrationState.Service?.PublishVehicleAdded(__result, !string.IsNullOrWhiteSpace(carId));
                RepeatedLogCoalescer.Flush("train-create-car-if-needed");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "train-create-car-if-needed",
                    "[ca.jwsm.railroader.api.host] TrainController.CreateCarIfNeeded postfix failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(TrainController), "WillRemoveCar")]
    internal static class TrainControllerWillRemoveCarPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Car car)
        {
            try
            {
                TrainIntegrationState.Service?.PublishVehicleRemoved(car);
                RepeatedLogCoalescer.Flush("train-will-remove-car");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "train-will-remove-car",
                    "[ca.jwsm.railroader.api.host] TrainController.WillRemoveCar prefix failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(TrainController), nameof(TrainController.IntegrationSetDidCouple))]
    internal static class TrainControllerIntegrationSetDidCouplePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Car car0, Car car1, float deltaVelocity, out bool __state)
        {
            try
            {
                __state = TrainIntegrationState.Service == null
                    || TrainIntegrationState.Service.PublishCouplerAttempt(car0, car1, deltaVelocity);
                RepeatedLogCoalescer.Flush("integration-set-did-couple-prefix");
                return __state;
            }
            catch (Exception ex)
            {
                __state = true;
                RepeatedLogCoalescer.LogWarning(
                    "integration-set-did-couple-prefix",
                    "[ca.jwsm.railroader.api.host] TrainController.IntegrationSetDidCouple prefix failed: " + ex);
                return true;
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Car car0, Car car1, float deltaVelocity, bool __state)
        {
            try
            {
                if (__state)
                {
                    TrainIntegrationState.Service?.PublishCoupled(car0, car1, deltaVelocity);
                }

                RepeatedLogCoalescer.Flush("integration-set-did-couple-postfix");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "integration-set-did-couple-postfix",
                    "[ca.jwsm.railroader.api.host] TrainController.IntegrationSetDidCouple postfix failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(TrainBrakeDisplay), "Awake")]
    internal static class TrainBrakeDisplayPatch
    {
        [HarmonyPostfix]
        private static void Postfix(TrainBrakeDisplay __instance)
        {
            try
            {
                TrainIntegrationState.Service?.PublishTrainBrakeDisplayAvailable(__instance);
                RepeatedLogCoalescer.Flush("train-brake-display-awake");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "train-brake-display-awake",
                    "[ca.jwsm.railroader.api.host] TrainBrakeDisplay.Awake postfix failed: " + ex);
            }
        }
    }

    [HarmonyPatch]
    internal static class IntegrationSetIntegrateConstraintsPatch
    {
        private sealed class IterationState
        {
            public float[] PositionsBefore;
            public float[] RadiiBefore;
            public int IterationsThisTick;
            public int LastAccumulatedFrame = -1;
            public bool PublishedThisTick;
        }

        private static readonly ConditionalWeakTable<IntegrationSet, IterationState> States = new ConditionalWeakTable<IntegrationSet, IterationState>();
        private static readonly Dictionary<Type, FieldInfo> ElementsFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly Dictionary<Type, ElementAccessor> ElementAccessorCache = new Dictionary<Type, ElementAccessor>();

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
                var service = TrainIntegrationState.Service;
                if (service == null || !service.HasConstraintTelemetrySubscribers)
                {
                    return;
                }

                var elements = GetElementsArray(__instance);
                if (elements == null || elements.Length < 2)
                {
                    return;
                }

                int count = elements.Length;
                var state = States.GetOrCreateValue(__instance);
                if (state.LastAccumulatedFrame == UnityEngine.Time.frameCount)
                {
                    return;
                }

                state.LastAccumulatedFrame = UnityEngine.Time.frameCount;
                state.IterationsThisTick = 0;
                state.PublishedThisTick = false;

                EnsureArray(ref state.PositionsBefore, count);
                EnsureArray(ref state.RadiiBefore, count);

                for (int i = 0; i < count; i++)
                {
                    object element = elements.GetValue(i);
                    var accessor = GetAccessor(element);
                    state.PositionsBefore[i] = accessor.GetPosition(element);
                    state.RadiiBefore[i] = accessor.GetRadius(element);
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
                var service = TrainIntegrationState.Service;
                if (service == null || !service.HasConstraintTelemetrySubscribers)
                {
                    return;
                }

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
                    || state.RadiiBefore == null)
                {
                    return;
                }

                int linkCount = elements.Length - 1;
                state.IterationsThisTick++;
                if (state.PublishedThisTick || state.IterationsThisTick < 4)
                {
                    return;
                }

                state.PublishedThisTick = true;
                var deltaSeparation = new float[linkCount];
                for (int i = 0; i < linkCount; i++)
                {
                    object firstElement = elements.GetValue(i);
                    object secondElement = elements.GetValue(i + 1);
                    var firstAccessor = GetAccessor(firstElement);
                    var secondAccessor = GetAccessor(secondElement);

                    float separationBefore = state.PositionsBefore[i + 1] - state.RadiiBefore[i + 1]
                        - (state.PositionsBefore[i] + state.RadiiBefore[i]);
                    float separationAfter = secondAccessor.GetPosition(secondElement) - secondAccessor.GetRadius(secondElement)
                        - (firstAccessor.GetPosition(firstElement) + firstAccessor.GetRadius(firstElement));
                    deltaSeparation[i] = separationAfter - separationBefore;
                }

                service.PublishConstraintTelemetry(__instance, wholeDeltaTime, deltaSeparation);
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

        private static ElementAccessor GetAccessor(object target)
        {
            var type = target.GetType();
            lock (ElementAccessorCache)
            {
                if (!ElementAccessorCache.TryGetValue(type, out var accessor))
                {
                    accessor = new ElementAccessor(type);
                    ElementAccessorCache[type] = accessor;
                }

                return accessor;
            }
        }

        private sealed class ElementAccessor
        {
            private readonly FieldInfo _positionField;
            private readonly PropertyInfo _positionProperty;
            private readonly FieldInfo _carRadiusField;
            private readonly PropertyInfo _carRadiusProperty;
            private readonly FieldInfo _radiusField;
            private readonly PropertyInfo _radiusProperty;

            public ElementAccessor(Type type)
            {
                _positionField = AccessTools.Field(type, "position");
                _positionProperty = AccessTools.Property(type, "position");
                _carRadiusField = AccessTools.Field(type, "CarRadius");
                _carRadiusProperty = AccessTools.Property(type, "CarRadius");
                _radiusField = AccessTools.Field(type, "radius");
                _radiusProperty = AccessTools.Property(type, "radius");
            }

            public float GetPosition(object target)
            {
                return GetValue(_positionField, _positionProperty, target);
            }

            public float GetRadius(object target)
            {
                float radius = GetValue(_carRadiusField, _carRadiusProperty, target);
                return Math.Abs(radius) > 1e-6f ? radius : GetValue(_radiusField, _radiusProperty, target);
            }

            private static float GetValue(FieldInfo field, PropertyInfo property, object target)
            {
                if (field != null)
                {
                    var fieldValue = field.GetValue(target);
                    if (fieldValue is float singleField)
                    {
                        return singleField;
                    }

                    if (fieldValue is double doubleField)
                    {
                        return (float)doubleField;
                    }
                }

                if (property != null)
                {
                    var propertyValue = property.GetValue(target, null);
                    if (propertyValue is float singleProperty)
                    {
                        return singleProperty;
                    }

                    if (propertyValue is double doubleProperty)
                    {
                        return (float)doubleProperty;
                    }
                }

                return 0f;
            }
        }
    }

    internal static class TrainIntegrationState
    {
        internal static TrainIntegrationService Service { get; set; }
    }
}
