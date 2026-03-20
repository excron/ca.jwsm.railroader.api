using System;
using System.Collections.Generic;
using System.Reflection;
using Ca.Jwsm.Railroader.Api.Host.Diagnostics;
using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using Game.State;
using HarmonyLib;
using Model;
using RollingStock;
using TMPro;
using UI;
using UI.Common;
using UI.ContextMenu;
using UnityEngine;
using ContextMenuUi = UI.ContextMenu.ContextMenu;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    [HarmonyPatch(typeof(CouplerPickable))]
    internal static class CouplerInteractionPatch
    {
        private static readonly FieldInfo QuadrantsField = AccessTools.Field(typeof(ContextMenuUi), "_quadrants");
        private static readonly FieldInfo CalloutTextLabelField = AccessTools.Field(typeof(Callout), "textLabel");
        private const string RightClickLinkId = "cf-right-click";
        private const string LeftClickOpenLine = "<sprite name=\"MouseLeft\"> Open Coupler";
        private static readonly string RightClickOpenLine = $"<link=\"{RightClickLinkId}\"><sprite name=\"MouseLeft\"></link> Open Menu";
        private const ContextMenuQuadrant LeftSlotQuadrant = ContextMenuQuadrant.Unused1;
        private const ContextMenuQuadrant RightSlotQuadrant = ContextMenuQuadrant.Unused2;

        [HarmonyPatch("get_ActivationFilter")]
        [HarmonyPostfix]
        private static void ActivationFilterPostfix(ref PickableActivationFilter __result)
        {
            try
            {
                __result = PickableActivationFilter.Any;
                RepeatedLogCoalescer.Flush("coupler-activation-filter");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "coupler-activation-filter",
                    "[ca.jwsm.railroader.api.host] Coupler activation filter patch failed: " + ex);
            }
        }

        [HarmonyPatch("get_TooltipInfo")]
        [HarmonyPostfix]
        private static void TooltipInfoPostfix(CouplerPickable __instance, ref TooltipInfo __result)
        {
            try
            {
                var service = CouplerInteractionState.Service;
                if (service == null || !TryCreateContext(__instance, out var context))
                {
                    return;
                }

                var tooltip = new CouplerTooltipContent(__result.Title, string.Empty);
                if (TryShouldShowOpenCouplerLine(__result.Text, context, out bool showOpenCoupler) && showOpenCoupler)
                {
                    tooltip.AppendLine(LeftClickOpenLine);
                }

                tooltip.AppendLine(RightClickOpenLine);
                service.PopulateTooltip(context, tooltip);
                __result = new TooltipInfo(tooltip.Title, tooltip.BuildText());
                RepeatedLogCoalescer.Flush("coupler-tooltip-info");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "coupler-tooltip-info",
                    "[ca.jwsm.railroader.api.host] Coupler tooltip patch failed: " + ex);
            }
        }

        [HarmonyPatch("Activate")]
        [HarmonyPrefix]
        private static bool ActivatePrefix(CouplerPickable __instance, PickableActivateEvent evt)
        {
            try
            {
                if (evt.Activation != PickableActivation.Secondary)
                {
                    return true;
                }

                var service = CouplerInteractionState.Service;
                if (service == null || !TryCreateContext(__instance, out var context))
                {
                    return true;
                }

                var menu = ContextMenuUi.Shared;
                if (menu == null)
                {
                    return false;
                }

                if (ContextMenuUi.IsShown)
                {
                    menu.Hide();
                }

                menu.Clear();

                var content = new CouplerMenuContent();
                service.PopulateMenu(context, content);
                for (int i = 0; i < content.Actions.Count; i++)
                {
                    AddMenuAction(menu, content.Actions[i]);
                }

                menu.Show(string.IsNullOrWhiteSpace(context.DisplayName) ? "Coupler" : context.DisplayName);
                RepeatedLogCoalescer.Flush("coupler-activate");
                return false;
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "coupler-activate",
                    "[ca.jwsm.railroader.api.host] Coupler activate patch failed: " + ex);
                return false;
            }
        }

        private static bool TryCreateContext(CouplerPickable pickable, out CouplerInteractionContext context)
        {
            context = null;
            if (pickable == null)
            {
                return false;
            }

            var car = pickable.GetComponentInParent<Car>();
            if (car == null || string.IsNullOrWhiteSpace(car.id))
            {
                return false;
            }

            Car.LogicalEnd logicalEnd = ResolveLogicalEnd(pickable, car);
            bool isFront = logicalEnd == Car.LogicalEnd.A ? car.FrontIsA : !car.FrontIsA;
            string endName = isFront ? "front" : "rear";
            context = new CouplerInteractionContext(
                new CouplerEndId(new VehicleId(car.id), endName),
                isFront,
                car.DisplayName ?? car.id,
                car,
                logicalEnd.ToString());
            return true;
        }

        private static void AddMenuAction(ContextMenuUi menu, CouplerMenuAction action)
        {
            if (menu == null || action == null)
            {
                return;
            }

            var quadrant = ResolveQuadrant(action);
            var spriteName = ResolveSprite(action.Style);

            menu.AddButton(quadrant, action.Label, spriteName, () =>
            {
                if (action.IsEnabled)
                {
                    action.OnSelected?.Invoke();
                }
                else if (!string.IsNullOrWhiteSpace(action.DisabledReason))
                {
                    PresentToast(action.DisabledReason);
                }
            });

            TryStyleLastButton(menu, quadrant, action.IsEnabled);
        }

        private static SpriteName ResolveSprite(CouplerActionStyle style)
        {
            switch (style)
            {
                case CouplerActionStyle.Toggle:
                    return SpriteName.Select;
                case CouplerActionStyle.Repair:
                    return SpriteName.Inspect;
                case CouplerActionStyle.Replace:
                    return SpriteName.Bleed;
                case CouplerActionStyle.Default:
                default:
                    return SpriteName.Select;
            }
        }

        private static ContextMenuQuadrant ResolveQuadrant(CouplerMenuAction action)
        {
            switch (action.Slot)
            {
                case CouplerMenuSlot.Left:
                    return LeftSlotQuadrant;
                case CouplerMenuSlot.Right:
                    return RightSlotQuadrant;
                case CouplerMenuSlot.General:
                    return ContextMenuQuadrant.General;
                case CouplerMenuSlot.Maintenance:
                    return ContextMenuQuadrant.Brakes;
                default:
                    return action.Group == CouplerActionGroup.Maintenance
                        ? ContextMenuQuadrant.Brakes
                        : ContextMenuQuadrant.General;
            }
        }

        private static void TryStyleLastButton(ContextMenuUi menu, ContextMenuQuadrant quadrant, bool isEnabled)
        {
            try
            {
                var quadrants = QuadrantsField == null ? null : QuadrantsField.GetValue(menu) as List<List<ContextMenuItem>>;
                if (quadrants == null)
                {
                    return;
                }

                int index = (int)quadrant;
                if (index < 0 || index >= quadrants.Count)
                {
                    return;
                }

                var list = quadrants[index];
                if (list == null || list.Count == 0)
                {
                    return;
                }

                var item = list[list.Count - 1];
                if (item == null)
                {
                    return;
                }

                if (isEnabled)
                {
                    item.label.color = WithAlpha(item.label.color, 1f);
                    item.image.color = WithAlpha(item.image.color, 1f);
                    item.colorDefault = WithAlpha(item.colorDefault, 1f);
                    item.colorHover = WithAlpha(item.colorHover, 1f);
                }
                else
                {
                    item.label.color = WithAlpha(item.label.color, 0.45f);
                    item.image.color = WithAlpha(item.image.color, 0.45f);
                    item.colorDefault = WithAlpha(item.colorDefault, 0.2f);
                    item.colorHover = WithAlpha(item.colorDefault, 0.3f);
                }
            }
            catch
            {
            }
        }

        private static Car.LogicalEnd ResolveLogicalEnd(CouplerPickable pickable, Car car)
        {
            try
            {
                var liveCoupler = pickable.GetComponent<Coupler>();
                if (liveCoupler != null)
                {
                    return car.EndToLogical(liveCoupler.end);
                }
            }
            catch
            {
            }

            return ResolveByPosition(car, pickable.transform.position);
        }

        private static Car.LogicalEnd ResolveByPosition(Car car, Vector3 position)
        {
            float distanceA = GetDistanceSquared(car, Car.LogicalEnd.A, position);
            float distanceB = GetDistanceSquared(car, Car.LogicalEnd.B, position);
            return distanceA <= distanceB ? Car.LogicalEnd.A : Car.LogicalEnd.B;
        }

        private static bool TryShouldShowOpenCouplerLine(string vanillaText, CouplerInteractionContext context, out bool showOpenCoupler)
        {
            showOpenCoupler = false;
            if (string.IsNullOrWhiteSpace(vanillaText))
            {
                return true;
            }

            var car = context?.NativeVehicle as Car;
            if (car == null)
            {
                return false;
            }

            if (!System.Enum.TryParse(context.NativeLogicalEnd, out Car.LogicalEnd logicalEnd))
            {
                logicalEnd = context.IsFront
                    ? (car.FrontIsA ? Car.LogicalEnd.A : Car.LogicalEnd.B)
                    : (car.FrontIsA ? Car.LogicalEnd.B : Car.LogicalEnd.A);
            }

            try
            {
                showOpenCoupler = car[logicalEnd].IsCoupled;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static float GetDistanceSquared(Car car, Car.LogicalEnd logicalEnd, Vector3 target)
        {
            try
            {
                var coupler = car[logicalEnd].Coupler;
                if (coupler == null)
                {
                    return float.MaxValue;
                }

                return (coupler.transform.position - target).sqrMagnitude;
            }
            catch
            {
                return float.MaxValue;
            }
        }

        [HarmonyPatch(typeof(Callout), nameof(Callout.SetTooltipInfo))]
        [HarmonyPostfix]
        private static void CalloutSetTooltipInfoPostfix(Callout __instance)
        {
            try
            {
                var textLabel = CalloutTextLabelField?.GetValue(__instance) as TMP_Text;
                if (textLabel == null || string.IsNullOrWhiteSpace(textLabel.text) || textLabel.text.IndexOf(RightClickLinkId, StringComparison.Ordinal) < 0)
                {
                    return;
                }

                textLabel.ForceMeshUpdate();
                var textInfo = textLabel.textInfo;
                if (textInfo == null || textInfo.linkCount <= 0)
                {
                    return;
                }

                bool touched = false;
                for (int linkIndex = 0; linkIndex < textInfo.linkCount; linkIndex++)
                {
                    var linkInfo = textInfo.linkInfo[linkIndex];
                    if (!string.Equals(linkInfo.GetLinkID(), RightClickLinkId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    for (int characterOffset = 0; characterOffset < linkInfo.linkTextLength; characterOffset++)
                    {
                        int charIndex = linkInfo.linkTextfirstCharacterIndex + characterOffset;
                        if (charIndex < 0 || charIndex >= textInfo.characterCount)
                        {
                            continue;
                        }

                        var charInfo = textInfo.characterInfo[charIndex];
                        if (!charInfo.isVisible)
                        {
                            continue;
                        }

                        int materialIndex = charInfo.materialReferenceIndex;
                        int vertexIndex = charInfo.vertexIndex;
                        var vertices = textInfo.meshInfo[materialIndex].vertices;
                        float centerX = (vertices[vertexIndex].x + vertices[vertexIndex + 2].x) * 0.5f;
                        for (int i = 0; i < 4; i++)
                        {
                            var vertex = vertices[vertexIndex + i];
                            vertex.x = centerX - (vertex.x - centerX);
                            vertices[vertexIndex + i] = vertex;
                        }

                        touched = true;
                    }
                }

                if (touched)
                {
                    textLabel.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
                }

                RepeatedLogCoalescer.Flush("coupler-callout-tooltip");
            }
            catch (Exception ex)
            {
                RepeatedLogCoalescer.LogWarning(
                    "coupler-callout-tooltip",
                    "[ca.jwsm.railroader.api.host] Coupler callout tooltip patch failed: " + ex);
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static void PresentToast(string message)
        {
            try
            {
                Toast.Present(message, ToastPosition.Middle);
            }
            catch
            {
            }
        }
    }

    internal static class CouplerInteractionState
    {
        internal static CouplerInteractionService Service { get; set; }
    }
}
