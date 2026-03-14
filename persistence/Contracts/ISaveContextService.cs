using Ca.Jwsm.Railroader.Api.Persistence.Models;

namespace Ca.Jwsm.Railroader.Api.Persistence.Contracts
{
    public interface ISaveContextService
    {
        SaveContext Current { get; }

        bool TryGetCurrent(out SaveContext context);

        void SetCurrent(SaveContext context);
    }
}
