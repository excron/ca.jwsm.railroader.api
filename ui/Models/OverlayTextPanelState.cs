namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class OverlayTextPanelState
    {
        public OverlayTextPanelState(string text, bool isVisible = true)
        {
            Text = text ?? string.Empty;
            IsVisible = isVisible;
        }

        public string Text { get; }

        public bool IsVisible { get; }
    }
}
