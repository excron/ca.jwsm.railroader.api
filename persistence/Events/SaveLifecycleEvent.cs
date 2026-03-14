using System;
using Ca.Jwsm.Railroader.Api.Persistence.Models;

namespace Ca.Jwsm.Railroader.Api.Persistence.Events
{
    public sealed class SaveLifecycleEvent
    {
        public SaveLifecycleEvent(
            SaveLifecycleStage stage,
            SaveContext context,
            DateTimeOffset? timestamp = null)
        {
            Stage = stage;
            Context = context ?? SaveContext.Empty;
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public SaveLifecycleStage Stage { get; }

        public SaveContext Context { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
