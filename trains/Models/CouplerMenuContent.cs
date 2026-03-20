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

        public int Count
        {
            get { return _actions.Count; }
        }

        public bool HasActions
        {
            get { return _actions.Count > 0; }
        }

        public void AddAction(
            string label,
            CouplerActionGroup group,
            CouplerMenuSlot slot,
            CouplerMenuIcon icon,
            bool isEnabled,
            Action onSelected,
            string disabledReason = null)
        {
            _actions.Add(new CouplerMenuAction(label, group, slot, icon, isEnabled, onSelected, disabledReason));
        }
    }
}
