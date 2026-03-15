using Ca.Jwsm.Railroader.Api.Abstractions.World.Models;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts
{
    public interface IWorldLayoutService
    {
        WorldLayoutStatus Status { get; }

        void Update(WorldLayoutSourceUpdate update, IWorldLayoutResolver resolver);

        void Clear(string sourceId);

        void Tick();

        void TryApplyEarly(string reason);
    }
}
