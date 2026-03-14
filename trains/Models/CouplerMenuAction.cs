using System;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerMenuAction
    {
        public CouplerMenuAction(
            string label,
            CouplerActionGroup group,
            CouplerActionStyle style,
            bool isEnabled,
            Action onSelected,
            string disabledReason = null)
        {
            Label = label ?? string.Empty;
            Group = group;
            Style = style;
            IsEnabled = isEnabled;
            OnSelected = onSelected;
            DisabledReason = disabledReason;
        }

        public string Label { get; }

        public CouplerActionGroup Group { get; }

        public CouplerActionStyle Style { get; }

        public bool IsEnabled { get; }

        public Action OnSelected { get; }

        public string DisabledReason { get; }
    }
}
