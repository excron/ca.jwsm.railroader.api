using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ControlContextService : IControlContextService
    {
        public bool TryGetSelected(out SelectedLocomotiveContext context)
        {
            context = null;
            return false;
        }
    }
}
