using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerMenuAction
    {
        public CouplerMenuAction(
            string label,
            CouplerActionGroup group,
            CouplerMenuSlot slot,
            CouplerMenuIcon icon,
            bool isEnabled,
            Action onSelected,
            string disabledReason = null)
        {
            Label = label ?? string.Empty;
            Group = group;
            Slot = slot;
            Icon = icon;
            IsEnabled = isEnabled;
            OnSelected = onSelected;
            DisabledReason = disabledReason;
        }

        public string Label { get; }

        public CouplerActionGroup Group { get; }

        public CouplerMenuSlot Slot { get; }

        public CouplerMenuIcon Icon { get; }

        public bool IsEnabled { get; }

        public Action OnSelected { get; }

        public string DisabledReason { get; }
    }
}
