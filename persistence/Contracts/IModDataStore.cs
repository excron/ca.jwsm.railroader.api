using Ca.Jwsm.Railroader.Api.Persistence.Models;

namespace Ca.Jwsm.Railroader.Api.Persistence.Contracts
{
    public interface IModDataStore
    {
        bool TryLoadJson(string ownerId, ModDataScope scope, ModDataKey key, out string json);

        void SaveJson(string ownerId, ModDataScope scope, ModDataKey key, string json);
    }
}
