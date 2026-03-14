using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using Model;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class TrainService : ITrainService, IConsistService
    {
        private static readonly TrainSnapshot EmptySnapshot = new TrainSnapshot(new TrainId("default"), "Train", new VehicleSnapshot[0]);

        public IEnumerable<TrainSnapshot> GetAll()
        {
            yield return BuildSnapshot();
        }

        public TrainSnapshot GetById(TrainId trainId)
        {
            return BuildSnapshot();
        }

        public ConsistSnapshot GetByTrainId(TrainId trainId)
        {
            var snapshot = BuildSnapshot();
            return new ConsistSnapshot(snapshot.Id, snapshot.Vehicles.Count);
        }

        private static TrainSnapshot BuildSnapshot()
        {
            try
            {
                var controller = TrainController.Shared;
                if (controller?.Cars == null)
                {
                    return EmptySnapshot;
                }

                var vehicles = new List<VehicleSnapshot>();
                foreach (var car in controller.Cars)
                {
                    if (car == null || string.IsNullOrWhiteSpace(car.id))
                    {
                        continue;
                    }

                    vehicles.Add(new VehicleSnapshot(
                        new VehicleId(car.id),
                        car.DisplayName ?? car.id,
                        car.GetType().Name.IndexOf("Locomotive", StringComparison.OrdinalIgnoreCase) >= 0));
                }

                return new TrainSnapshot(new TrainId("default"), "Train", vehicles);
            }
            catch
            {
                return EmptySnapshot;
            }
        }
    }
}
