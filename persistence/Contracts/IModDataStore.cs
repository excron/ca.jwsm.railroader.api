using Ca.Jwsm.Railroader.Api.Persistence.Models;

namespace Ca.Jwsm.Railroader.Api.Persistence.Contracts
{
    public interface IModDataStore
    {
        bool TryLoadJson(string ownerId, ModDataScope scope, ModDataKey key, out string json);

        void SaveJson(string ownerId, ModDataScope scope, ModDataKey key, string json);

        bool TryLoad<T>(string ownerId, ModDataScope scope, ModDataKey key, out T value);

        void Save<T>(string ownerId, ModDataScope scope, ModDataKey key, T value);

        void Delete(string ownerId, ModDataScope scope, ModDataKey key, string scopeId = null);
    }
}
