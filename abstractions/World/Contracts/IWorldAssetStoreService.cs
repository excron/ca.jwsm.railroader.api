using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.World.Models;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts
{
    public interface IWorldAssetStoreService
    {
        WorldAssetStoreStatus Status { get; }

        void Update(string sourceId, IReadOnlyList<WorldAssetStoreRegistration> registrations);

        void Clear(string sourceId);
    }
}
