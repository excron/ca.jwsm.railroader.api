namespace Ca.Jwsm.Railroader.Api.Ui.Models
{
    public sealed class HudContext
    {
        public HudContext(
            string selectedSubjectId = null,
            string selectedDisplayName = null,
            HudControlSetKind controlSetKind = HudControlSetKind.Unknown,
            bool isVisible = false)
        {
            SelectedSubjectId = selectedSubjectId;
            SelectedDisplayName = selectedDisplayName;
            ControlSetKind = controlSetKind;
            IsVisible = isVisible;
        }

        public string SelectedSubjectId { get; }

        public string SelectedDisplayName { get; }

        public HudControlSetKind ControlSetKind { get; }

        public bool IsVisible { get; }
    }
}
