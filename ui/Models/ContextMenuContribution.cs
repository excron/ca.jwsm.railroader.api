using System;

namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class ContextMenuContribution
    {
        public ContextMenuContribution(string id, string label, UiPriority priority, Action onSelected = null)
        {
            Id = id;
            Label = label;
            Priority = priority;
            OnSelected = onSelected;
        }

        public string Id { get; }

        public string Label { get; }

        public UiPriority Priority { get; }

        public Action OnSelected { get; }
    }
}
