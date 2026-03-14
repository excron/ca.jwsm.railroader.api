using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Ui.Contracts
{
    public interface IHudContextService
    {
        bool TryGetCurrent(out HudContext context);
    }
}
