using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerMenuContent
    {
        private readonly List<CouplerMenuAction> _actions = new List<CouplerMenuAction>();

        public IReadOnlyList<CouplerMenuAction> Actions
        {
            get { return _actions; }
        }

        public void AddAction(
            string label,
            CouplerActionGroup group,
            CouplerMenuSlot slot,
            CouplerActionStyle style,
            bool isEnabled,
            Action onSelected,
            string disabledReason = null)
        {
            _actions.Add(new CouplerMenuAction(label, group, slot, style, isEnabled, onSelected, disabledReason));
        }
    }
}
