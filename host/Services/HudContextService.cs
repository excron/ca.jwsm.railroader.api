using Ca.Jwsm.Railroader.Api.Ui.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class HudContextService : IHudContextService
    {
        public bool TryGetCurrent(out HudContext context)
        {
            context = null;
            return false;
        }
    }
}
