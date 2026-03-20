namespace Ca.Jwsm.Railroader.Api.Persistence.Models
{
    public enum SaveLifecycleStage
    {
        Unknown = 0,
        Loading = 1,
        Loaded = 2,
        Saving = 3,
        Saved = 4,
        Unloading = 5,
        ApplicationQuitting = 6,
        Deleted = 7
    }
}
