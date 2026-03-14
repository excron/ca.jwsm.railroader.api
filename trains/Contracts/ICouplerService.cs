using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface ICouplerService
    {
        CouplerStateSnapshot GetState(CouplerEndId couplerEndId);
    }
}
