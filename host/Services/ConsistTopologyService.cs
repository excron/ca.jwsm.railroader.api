using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ConsistTopologyService : IConsistTopologyService
    {
        public IReadOnlyList<ConsistGroupSnapshot> GetGroups(VehicleId selectedVehicleId)
        {
            return new ConsistGroupSnapshot[0];
        }
    }
}
