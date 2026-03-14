using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface IConsistTopologyService
    {
        IReadOnlyList<ConsistGroupSnapshot> GetGroups(VehicleId selectedVehicleId);
    }
}
