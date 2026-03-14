namespace Ca.Jwsm.Railroader.Api.Persistence.Models
{
    public sealed class SaveContext
    {
        public static readonly SaveContext Empty = new SaveContext(string.Empty, string.Empty);

        public SaveContext(string saveId, string displayName)
        {
            SaveId = saveId;
            DisplayName = displayName;
        }

        public string SaveId { get; }

        public string DisplayName { get; }
    }
}
