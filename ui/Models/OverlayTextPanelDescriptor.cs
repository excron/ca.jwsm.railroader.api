using System;

namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class OverlayTextPanelDescriptor
    {
        public OverlayTextPanelDescriptor(
            string id,
            string title,
            OverlayTextAnchor anchor,
            UiPriority priority,
            Func<OverlayTextPanelState> stateProvider,
            float offsetX = 16f,
            float offsetY = 16f,
            float minWidth = 220f)
        {
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            Anchor = anchor;
            Priority = priority;
            StateProvider = stateProvider;
            OffsetX = offsetX;
            OffsetY = offsetY;
            MinWidth = minWidth;
        }

        public string Id { get; }

        public string Title { get; }

        public OverlayTextAnchor Anchor { get; }

        public UiPriority Priority { get; }

        public Func<OverlayTextPanelState> StateProvider { get; }

        public float OffsetX { get; }

        public float OffsetY { get; }

        public float MinWidth { get; }
    }
}
