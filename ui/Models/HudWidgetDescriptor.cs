using System;

namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class HudWidgetDescriptor
    {
        public HudWidgetDescriptor(
            string id,
            string title,
            UiAnchorId anchor,
            UiPriority priority,
            string description = null,
            Func<HudContext, bool> isVisible = null,
            Action render = null)
        {
            Id = id;
            Title = title;
            Anchor = anchor;
            Priority = priority;
            Description = description;
            IsVisible = isVisible;
            Render = render;
        }

        public string Id { get; }

        public string Title { get; }

        public UiAnchorId Anchor { get; }

        public UiPriority Priority { get; }

        public string Description { get; }

        public Func<HudContext, bool> IsVisible { get; }

        public Action Render { get; }
    }
}
