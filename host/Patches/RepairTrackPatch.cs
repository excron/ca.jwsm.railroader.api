using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Host.Diagnostics;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using HarmonyLib;
using Model;
using Model.Ops;
using Model.Ops.Definition;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    [HarmonyPatch(typeof(RepairTrack), "TickCar")]
    internal static class RepairTrackTickCarPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Car car, out float __state)
        {
            try
            {
                __state = car != null ? car.Condition : 0f;
                RepeatedLogCoalescer.Flush("repair-track-tick-prefix");
            }
            catch (System.Exception ex)
            {
                __state = 0f;
                RepeatedLogCoalescer.LogWarning(
                    "repair-track-tick-prefix",
                    "[ca.jwsm.railroader.api.host] RepairTrack.TickCar prefix failed: " + ex);
            }
        }

        [HarmonyPostfix]
        private static void Postfix(Car car, float __state, bool __result, float repairUsed)
        {
            try
            {
                if (!__result || car == null || repairUsed <= 1e-6f)
                {
                    return;
                }

                TrainIntegrationState.Service?.PublishVehicleRepairProgressed(car, __state, car.Condition, repairUsed);
                RepeatedLogCoalescer.Flush("repair-track-tick-postfix");
            }
            catch (System.Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "repair-track-tick-postfix",
                    "[ca.jwsm.railroader.api.host] RepairTrack.TickCar postfix failed: " + ex);
            }
        }
    }

    [HarmonyPatch(typeof(RepairTrack), nameof(RepairTrack.Service))]
    internal static class RepairTrackServicePatch
    {
        private static readonly MethodInfo RepairTrackCalculateRepairStepMethod = AccessTools.Method(typeof(RepairTrack), "CalculateRepairStep");
        private static readonly MethodInfo RepairTrackEquipmentRepairSpeedMethod = AccessTools.Method(typeof(RepairTrack), "EquipmentRepairSpeed");
        private static readonly MethodInfo CarArchetypeGetter = AccessTools.PropertyGetter(typeof(Car), "Archetype");
        private static readonly MethodInfo EffectiveRepairPerDayPerCarMethod = AccessTools.Method(typeof(RepairTrack), "EffectiveRepairPerDayPerCar");
        private static readonly MethodInfo EnumerateCarsActualMethod = AccessTools.Method(typeof(RepairTrack), "EnumerateCarsActual");
        private static readonly MethodInfo NeedsRepairMethod = AccessTools.Method(typeof(RepairTrack), "NeedsRepair");
        private static readonly MethodInfo RateToValueMethod = AccessTools.Method(typeof(IndustryComponent), "RateToValue");
        private static readonly FieldInfo RepairPartsLoadField = AccessTools.Field(typeof(RepairTrack), "repairPartsLoad");

        [HarmonyPostfix]
        private static void Postfix(RepairTrack __instance, IIndustryContext ctx)
        {
            try
            {
                var service = TrainIntegrationState.Service;
                if (__instance == null || ctx == null || service == null)
                {
                    return;
                }

                float repairWorkAvailable = GetRepairWorkAvailablePerCar(__instance, ctx);
                if (repairWorkAvailable <= 1e-6f || !HasRepairPartsAvailable(__instance, ctx))
                {
                    return;
                }

                foreach (var car in EnumerateCars(__instance, ctx).OrderBy(c => c.id))
                {
                    if (car == null || NeedsRepair(car))
                    {
                        continue;
                    }

                    var repairEstimate = CreateRepairEstimate(car, repairWorkAvailable);
                    if (repairEstimate == null)
                    {
                        continue;
                    }

                    service.PublishVehicleRepairWorkAvailable(car, repairEstimate);
                }

                RepeatedLogCoalescer.Flush("repair-track-service-postfix");
            }
            catch (System.Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "repair-track-service-postfix",
                    "[ca.jwsm.railroader.api.host] RepairTrack.Service postfix failed: " + ex);
            }
        }

        private static float GetRepairWorkAvailablePerCar(RepairTrack repairTrack, IIndustryContext ctx)
        {
            if (EffectiveRepairPerDayPerCarMethod == null)
            {
                return 0f;
            }

            try
            {
                object[] args = { 0f };
                object result = EffectiveRepairPerDayPerCarMethod.Invoke(repairTrack, args);
                float repairPerDay = result is float single ? single : 0f;
                if (RateToValueMethod == null)
                {
                    return 0f;
                }

                object rateValue = RateToValueMethod.Invoke(null, new object[] { repairPerDay, ctx.DeltaTime });
                return rateValue is float repairWorkUnits ? repairWorkUnits : 0f;
            }
            catch
            {
                return 0f;
            }
        }

        private static RepairWorkEstimate CreateRepairEstimate(Car car, float repairWorkUnitsAvailable)
        {
            if (car == null || repairWorkUnitsAvailable <= 1e-6f)
            {
                return null;
            }

            return new RepairWorkEstimate(
                repairWorkUnitsAvailable,
                currentCondition => EstimateConditionDelta(car, currentCondition, repairWorkUnitsAvailable));
        }

        private static float EstimateConditionDelta(Car car, float currentCondition, float repairWorkUnitsAvailable)
        {
            if (car == null
                || repairWorkUnitsAvailable <= 1e-6f
                || RepairTrackCalculateRepairStepMethod == null
                || RepairTrackEquipmentRepairSpeedMethod == null)
            {
                return 0f;
            }

            try
            {
                float repairSpeed = RepairTrackEquipmentRepairSpeedMethod.Invoke(null, new object[] { car }) is float speed
                    ? speed
                    : 0f;
                if (repairSpeed <= 1e-6f)
                {
                    return 0f;
                }

                object archetype = CarArchetypeGetter?.Invoke(car, null);
                if (archetype == null)
                {
                    return 0f;
                }

                object[] args =
                {
                    Mathf.Clamp01(currentCondition),
                    repairWorkUnitsAvailable,
                    1f,
                    repairSpeed,
                    archetype,
                    0f,
                    0f
                };

                RepairTrackCalculateRepairStepMethod.Invoke(null, args);
                float resultingCondition = args[5] is float result ? result : Mathf.Clamp01(currentCondition);
                return Mathf.Max(0f, resultingCondition - Mathf.Clamp01(currentCondition));
            }
            catch
            {
                return 0f;
            }
        }

        private static bool HasRepairPartsAvailable(RepairTrack repairTrack, IIndustryContext ctx)
        {
            if (RepairPartsLoadField == null)
            {
                return true;
            }

            try
            {
                var repairPartsLoad = RepairPartsLoadField.GetValue(repairTrack);
                if (repairPartsLoad == null)
                {
                    return false;
                }

                return ctx.QuantityInStorage((Load)repairPartsLoad) > 1e-3f;
            }
            catch
            {
                return true;
            }
        }

        private static IEnumerable<Car> EnumerateCars(RepairTrack repairTrack, IIndustryContext ctx)
        {
            if (EnumerateCarsActualMethod == null)
            {
                yield break;
            }

            IEnumerator enumerator = null;
            try
            {
                enumerator = (EnumerateCarsActualMethod.Invoke(repairTrack, new object[] { ctx }) as IEnumerable)?.GetEnumerator();
            }
            catch
            {
            }

            if (enumerator == null)
            {
                yield break;
            }

            using (enumerator as System.IDisposable)
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current is Car car && car != null)
                    {
                        yield return car;
                    }
                }
            }
        }

        private static bool NeedsRepair(Car car)
        {
            if (car == null || NeedsRepairMethod == null)
            {
                return false;
            }

            try
            {
                return NeedsRepairMethod.Invoke(null, new object[] { car }) is bool needsRepair && needsRepair;
            }
            catch
            {
                return false;
            }
        }
    }
}
