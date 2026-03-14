using System;
using System.Collections.Generic;
using System.Reflection;
using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using Game.State;
using HarmonyLib;
using Model;
using RollingStock;
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

        [HarmonyPatch("get_ActivationFilter")]
        [HarmonyPostfix]
        private static void ActivationFilterPostfix(ref PickableActivationFilter __result)
        {
            __result = PickableActivationFilter.Any;
        }

        [HarmonyPatch("get_TooltipInfo")]
        [HarmonyPostfix]
        private static void TooltipInfoPostfix(CouplerPickable __instance, ref TooltipInfo __result)
        {
            var service = CouplerInteractionState.Service;
            if (service == null || !TryCreateContext(__instance, out var context))
            {
                return;
            }

            var tooltip = new CouplerTooltipContent(__result.Title, __result.Text);
            service.PopulateTooltip(context, tooltip);
            __result = new TooltipInfo(tooltip.Title, tooltip.BuildText());
        }

        [HarmonyPatch("Activate")]
        [HarmonyPrefix]
        private static bool ActivatePrefix(CouplerPickable __instance, PickableActivateEvent evt)
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
            AddToggleAction(menu, __instance);

            var content = new CouplerMenuContent();
            service.PopulateMenu(context, content);
            for (int i = 0; i < content.Actions.Count; i++)
            {
                AddMenuAction(menu, content.Actions[i]);
            }

            menu.Show(string.IsNullOrWhiteSpace(context.DisplayName) ? "Coupler" : context.DisplayName);
            return false;
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

        private static void AddToggleAction(ContextMenuUi menu, CouplerPickable pickable)
        {
            string title = pickable.isOpen ? "Close Coupler" : "Open Coupler";
            menu.AddButton(ContextMenuQuadrant.General, title, SpriteName.Select, () =>
            {
                try
                {
                    pickable.activate?.Invoke();
                }
                catch
                {
                }
            });
        }

        private static void AddMenuAction(ContextMenuUi menu, CouplerMenuAction action)
        {
            if (menu == null || action == null)
            {
                return;
            }

            var quadrant = action.Group == CouplerActionGroup.Maintenance
                ? ContextMenuQuadrant.Brakes
                : ContextMenuQuadrant.General;
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
