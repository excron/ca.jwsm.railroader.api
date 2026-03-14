using System;

namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class WindowDescriptor
    {
        public WindowDescriptor(string id, string title, UiAnchorId anchor, UiPriority priority, Action onOpen = null)
        {
            Id = id;
            Title = title;
            Anchor = anchor;
            Priority = priority;
            OnOpen = onOpen;
        }

        public string Id { get; }

        public string Title { get; }

        public UiAnchorId Anchor { get; }

        public UiPriority Priority { get; }

        public Action OnOpen { get; }
    }
}
