using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface IConsistService
    {
        ConsistSnapshot GetByTrainId(TrainId trainId);
    }
}
