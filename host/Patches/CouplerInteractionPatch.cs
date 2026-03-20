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
using UnityEngine.UI;
using ContextMenuUi = UI.ContextMenu.ContextMenu;

namespace Ca.Jwsm.Railroader.Api.Host.Patches
{
    [HarmonyPatch(typeof(CouplerPickable))]
    internal static class CouplerInteractionPatch
    {
        private static readonly FieldInfo QuadrantsField = AccessTools.Field(typeof(ContextMenuUi), "_quadrants");
        private static readonly FieldInfo DividersField = AccessTools.Field(typeof(ContextMenuUi), "_dividers");
        private static readonly FieldInfo ContentRectTransformField = AccessTools.Field(typeof(ContextMenuUi), "contentRectTransform");
        private static readonly FieldInfo RadiusField = AccessTools.Field(typeof(ContextMenuUi), "radius");
        private static readonly FieldInfo CalloutTextLabelField = AccessTools.Field(typeof(Callout), "textLabel");
        private const string RightClickLinkId = "cf-right-click";
        private const string LeftClickOpenLine = "<sprite name=\"MouseLeft\"> Open Coupler";
        private static readonly string RightClickOpenLine = $"<link=\"{RightClickLinkId}\"><sprite name=\"MouseLeft\"></link> Open Menu";
        private const ContextMenuQuadrant LeftSlotQuadrant = ContextMenuQuadrant.Unused1;
        private const ContextMenuQuadrant RightSlotQuadrant = ContextMenuQuadrant.Unused2;
        private const string TopDividerName = "ApiTwoButtonDividerTop";
        private const string BottomDividerName = "ApiTwoButtonDividerBottom";

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

                var menuContent = new CouplerMenuContent();
                service.PopulateMenu(context, menuContent);

                var tooltip = new CouplerTooltipContent(BuildTooltipTitle(context, __result.Title), string.Empty);
                if (TryShouldShowOpenCouplerLine(__result.Text, context, out bool showOpenCoupler) && showOpenCoupler)
                {
                    tooltip.AppendLine(LeftClickOpenLine);
                }

                if (menuContent.HasActions)
                {
                    tooltip.AppendLine(RightClickOpenLine);
                }
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
                if (!content.HasActions)
                {
                    RepeatedLogCoalescer.Flush("coupler-activate");
                    return false;
                }

                for (int i = 0; i < content.Actions.Count; i++)
                {
                    AddMenuAction(menu, content.Actions[i]);
                }

                menu.Show(string.IsNullOrWhiteSpace(context.DisplayName) ? "Coupler" : context.DisplayName);
                TryApplyExplicitTwoButtonLayout(menu, content);
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

        private static void TryApplyExplicitTwoButtonLayout(ContextMenuUi menu, CouplerMenuContent content)
        {
            if (menu == null)
            {
                return;
            }

            var contentRt = ContentRectTransformField?.GetValue(menu) as RectTransform;
            if (content == null || content.Count != 2)
            {
                HideTwoButtonBorders(contentRt);
                return;
            }

            try
            {
                var quadrants = QuadrantsField?.GetValue(menu) as List<List<ContextMenuItem>>;
                if (quadrants == null || contentRt == null)
                {
                    return;
                }

                var leftItems = quadrants[(int)LeftSlotQuadrant];
                var rightItems = quadrants[(int)RightSlotQuadrant];
                if (leftItems == null || rightItems == null || leftItems.Count != 1 || rightItems.Count != 1)
                {
                    return;
                }

                float radius = RadiusField?.GetValue(menu) is float configuredRadius ? configuredRadius : 100f;
                ApplyExplicitItemLayout(leftItems[0], 180f, 135f, 90f, radius, new Vector2(1f, 0.5f));
                ApplyExplicitItemLayout(rightItems[0], 0f, 315f, 90f, radius, new Vector2(0f, 0.5f));

                var dividers = DividersField?.GetValue(menu) as List<RectTransform>;
                if (dividers != null)
                {
                    for (int i = 0; i < dividers.Count; i++)
                    {
                        var divider = dividers[i];
                        if (divider != null)
                        {
                            divider.gameObject.SetActive(false);
                        }
                    }
                }

                ApplyTwoButtonBorders(contentRt, radius, ResolveDividerColor(dividers));
            }
            catch
            {
                HideTwoButtonBorders(contentRt);
            }
        }

        private static void ApplyTwoButtonBorders(RectTransform contentRt, float radius, Color color)
        {
            if (contentRt == null)
            {
                return;
            }

            var topDivider = EnsureTwoButtonDivider(contentRt, TopDividerName, color);
            var bottomDivider = EnsureTwoButtonDivider(contentRt, BottomDividerName, color);
            if (topDivider == null || bottomDivider == null)
            {
                return;
            }

            const float dividerThickness = 3f;
            float dividerLength = Mathf.Max(28f, radius * 0.72f);
            float dividerOffset = Mathf.Max(18f, radius * 0.57f);

            ApplyTwoButtonDividerLayout(topDivider, dividerThickness, dividerLength, dividerOffset);
            ApplyTwoButtonDividerLayout(bottomDivider, dividerThickness, dividerLength, -dividerOffset);
        }

        private static RectTransform EnsureTwoButtonDivider(RectTransform parent, string name, Color color)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null)
            {
                var existingImage = existing.GetComponent<Image>();
                if (existingImage != null)
                {
                    existingImage.color = color;
                    existingImage.raycastTarget = false;
                }

                existing.gameObject.SetActive(true);
                return existing;
            }

            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;

            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            return rect;
        }

        private static void HideTwoButtonBorders(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            TryHideChild(parent, TopDividerName);
            TryHideChild(parent, BottomDividerName);
        }

        private static void TryHideChild(RectTransform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private static void ApplyTwoButtonDividerLayout(RectTransform divider, float thickness, float length, float offsetY)
        {
            if (divider == null)
            {
                return;
            }

            divider.sizeDelta = new Vector2(thickness, length);
            divider.anchoredPosition = new Vector2(0f, offsetY);
            divider.localRotation = Quaternion.identity;
            divider.gameObject.SetActive(true);
        }

        private static Color ResolveDividerColor(List<RectTransform> dividers)
        {
            if (dividers != null)
            {
                for (int i = 0; i < dividers.Count; i++)
                {
                    var divider = dividers[i];
                    if (divider == null)
                    {
                        continue;
                    }

                    var image = divider.GetComponent<Image>();
                    if (image != null)
                    {
                        return image.color;
                    }
                }
            }

            return new Color(0.79f, 0.72f, 0.56f, 0.9f);
        }

        private static void ApplyExplicitItemLayout(
            ContextMenuItem item,
            float centerAngleDegrees,
            float startAngleDegrees,
            float angleRangeDegrees,
            float radius,
            Vector2 textPivot)
        {
            if (item == null)
            {
                return;
            }

            item.SetAngle(startAngleDegrees, angleRangeDegrees);

            Vector2 iconPosition = PositionForAngle(centerAngleDegrees, radius);
            ((RectTransform)item.transform).localPosition = iconPosition;
            item.textContainer.pivot = textPivot;
            item.textContainer.anchoredPosition = PositionForAngle(centerAngleDegrees, radius * 1.5f) - iconPosition;
            ((RectTransform)item.wedgeImage.transform).localPosition = -iconPosition;
        }

        private static Vector2 PositionForAngle(float angleDegrees, float radius)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians) * radius, Mathf.Sin(radians) * radius);
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

        private static string BuildTooltipTitle(CouplerInteractionContext context, string fallbackTitle)
        {
            string baseTitle = string.IsNullOrWhiteSpace(fallbackTitle) ? "Coupler" : fallbackTitle.Trim();
            if (context == null || string.IsNullOrWhiteSpace(context.DisplayName))
            {
                return baseTitle;
            }

            return $"{baseTitle} [{context.DisplayName.Trim()}]";
        }

        private static void AddMenuAction(ContextMenuUi menu, CouplerMenuAction action)
        {
            if (menu == null || action == null)
            {
                return;
            }

            var quadrant = ResolveQuadrant(action);
            var spriteName = ResolveSprite(action.Icon);

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

        private static SpriteName ResolveSprite(CouplerMenuIcon icon)
        {
            switch (icon)
            {
                case CouplerMenuIcon.Select:
                    return SpriteName.Select;
                case CouplerMenuIcon.Inspect:
                    return SpriteName.Inspect;
                case CouplerMenuIcon.Bleed:
                    return SpriteName.Bleed;
                case CouplerMenuIcon.Default:
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
