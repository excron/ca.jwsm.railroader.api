using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface ITrainService
    {
        IEnumerable<TrainSnapshot> GetAll();

        TrainSnapshot GetById(TrainId trainId);
    }
}
