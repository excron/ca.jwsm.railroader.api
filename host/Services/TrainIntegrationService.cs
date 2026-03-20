using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Trains.Events;
using Ca.Jwsm.Railroader.Api.Trains.Models;
using Ca.Jwsm.Railroader.Api.Ui.Events;
using Model;
using Model.Physics;
using UI;
using UnityEngine;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class TrainIntegrationService
    {
        [ThreadStatic]
        private static VehicleSpawnReason _currentSpawnReason;

        private readonly IEventBus _events;

        public TrainIntegrationService(IEventBus events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            _events = events;
        }

        public IDisposable PushSpawnReason(VehicleSpawnReason spawnReason)
        {
            var previous = _currentSpawnReason;
            _currentSpawnReason = spawnReason;
            return new SpawnReasonScope(previous);
        }

        public bool HasConstraintTelemetrySubscribers
        {
            get { return _events.HasSubscribers<ConstraintTelemetryCapturedEvent>(); }
        }

        public void PublishVehicleAdded(Car car, bool hadRequestedCarId = false)
        {
            if (!TryCreateVehicleId(car, out var vehicleId))
            {
                return;
            }

            _events.Publish(new VehicleAddedEvent(vehicleId, car.DisplayName ?? vehicleId.Value, ResolveSpawnReason(hadRequestedCarId)));
        }

        public void PublishVehicleRemoved(Car car)
        {
            if (!TryCreateVehicleId(car, out var vehicleId))
            {
                return;
            }

            _events.Publish(new VehicleRemovedEvent(vehicleId, car.DisplayName ?? vehicleId.Value));
        }

        public bool PublishCouplerAttempt(Car firstCar, Car secondCar, float deltaVelocity)
        {
            if (!TryResolveCouplerAttempt(firstCar, secondCar, deltaVelocity, out var attempt, out var firstEnd, out var secondEnd))
            {
                return true;
            }

            _events.Publish(attempt);
            if (!attempt.ShouldDisconnect)
            {
                return true;
            }

            DisconnectEnd(firstCar, firstEnd);
            DisconnectEnd(secondCar, secondEnd);
            _events.Publish(new UncoupledEvent(attempt.First, attempt.Second));
            return false;
        }

        public void PublishCoupled(Car firstCar, Car secondCar, float deltaVelocity)
        {
            if (!TryResolveCouplerAttempt(firstCar, secondCar, deltaVelocity, out var attempt, out _, out _))
            {
                return;
            }

            _events.Publish(new CoupledEvent(attempt.First, attempt.Second));
        }

        public void PublishConstraintTelemetry(IntegrationSet integrationSet, float deltaTimeSeconds, float[] deltaSeparationMeters)
        {
            if (integrationSet == null || deltaTimeSeconds <= 0f || deltaSeparationMeters == null || deltaSeparationMeters.Length == 0)
            {
                return;
            }

            _events.Publish(new ConstraintTelemetryCapturedEvent(integrationSet, deltaTimeSeconds, deltaSeparationMeters));
        }

        public void PublishTrainBrakeDisplayAvailable(TrainBrakeDisplay display)
        {
            if (display == null)
            {
                return;
            }

            _events.Publish(new TrainBrakeDisplayAvailableEvent(display));
        }

        public void PublishVehicleRepairProgressed(Car car, float conditionBefore, float conditionAfter, float repairUsed)
        {
            if (!TryCreateVehicleId(car, out var vehicleId) || repairUsed <= 1e-6f)
            {
                return;
            }

            if ((conditionAfter - conditionBefore) <= 1e-6f)
            {
                return;
            }

            _events.Publish(new VehicleRepairProgressedEvent(
                vehicleId,
                car.DisplayName ?? vehicleId.Value,
                conditionBefore,
                conditionAfter,
                repairUsed,
                car));
        }

        public void PublishVehicleRepairWorkAvailable(Car car, float repairWorkUnitsAvailable)
        {
            if (!TryCreateVehicleId(car, out var vehicleId) || repairWorkUnitsAvailable <= 1e-6f)
            {
                return;
            }

            _events.Publish(new VehicleRepairWorkAvailableEvent(
                vehicleId,
                car.DisplayName ?? vehicleId.Value,
                repairWorkUnitsAvailable,
                car));
        }

        private static VehicleSpawnReason ResolveSpawnReason(bool hadRequestedCarId)
        {
            if (_currentSpawnReason != VehicleSpawnReason.Unknown)
            {
                return _currentSpawnReason;
            }

            return hadRequestedCarId
                ? VehicleSpawnReason.InterchangeUsed
                : VehicleSpawnReason.PurchasedNew;
        }

        private static bool TryCreateVehicleId(Car car, out VehicleId vehicleId)
        {
            vehicleId = default(VehicleId);
            if (car == null || string.IsNullOrWhiteSpace(car.id))
            {
                return false;
            }

            vehicleId = new VehicleId(car.id);
            return true;
        }

        private static bool TryResolveCouplerAttempt(
            Car firstCar,
            Car secondCar,
            float deltaVelocity,
            out CouplerAttemptedEvent attempt,
            out Car.LogicalEnd firstEnd,
            out Car.LogicalEnd secondEnd)
        {
            attempt = null;
            firstEnd = Car.LogicalEnd.A;
            secondEnd = Car.LogicalEnd.A;

            if (!TryCreateVehicleId(firstCar, out var firstVehicleId) || !TryCreateVehicleId(secondCar, out var secondVehicleId))
            {
                return false;
            }

            if (!TryResolveFacingEnds(firstCar, secondCar, out firstEnd, out secondEnd))
            {
                return false;
            }

            attempt = new CouplerAttemptedEvent(
                new CouplerEndId(firstVehicleId, ToEndName(firstCar, firstEnd)),
                new CouplerEndId(secondVehicleId, ToEndName(secondCar, secondEnd)),
                deltaVelocity * 2.23694f);
            return true;
        }

        private static bool TryResolveFacingEnds(Car firstCar, Car secondCar, out Car.LogicalEnd firstEnd, out Car.LogicalEnd secondEnd)
        {
            firstEnd = Car.LogicalEnd.A;
            secondEnd = Car.LogicalEnd.A;

            float bestDistance = float.MaxValue;
            bool found = false;
            foreach (var firstCandidate in EnumerateEnds())
            {
                if (!TryGetCouplerPosition(firstCar, firstCandidate, out var firstPosition))
                {
                    continue;
                }

                foreach (var secondCandidate in EnumerateEnds())
                {
                    if (!TryGetCouplerPosition(secondCar, secondCandidate, out var secondPosition))
                    {
                        continue;
                    }

                    float distance = (firstPosition - secondPosition).sqrMagnitude;
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    bestDistance = distance;
                    firstEnd = firstCandidate;
                    secondEnd = secondCandidate;
                    found = true;
                }
            }

            return found;
        }

        private static IEnumerable<Car.LogicalEnd> EnumerateEnds()
        {
            yield return Car.LogicalEnd.A;
            yield return Car.LogicalEnd.B;
        }

        private static bool TryGetCouplerPosition(Car car, Car.LogicalEnd logicalEnd, out Vector3 position)
        {
            position = Vector3.zero;
            try
            {
                var coupler = car[logicalEnd].Coupler;
                if (coupler == null)
                {
                    return false;
                }

                position = coupler.transform.position;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ToEndName(Car car, Car.LogicalEnd logicalEnd)
        {
            bool isFront = logicalEnd == Car.LogicalEnd.A ? car.FrontIsA : !car.FrontIsA;
            return isFront ? "front" : "rear";
        }

        private static void DisconnectEnd(Car car, Car.LogicalEnd logicalEnd)
        {
            if (car == null)
            {
                return;
            }

            try
            {
                var endGear = car[logicalEnd];
                if (endGear.IsCoupled)
                {
                    car.ApplyEndGearChange(logicalEnd, Car.EndGearStateKey.IsCoupled, boolValue: false);
                }

                if (endGear.IsAirConnected)
                {
                    car.ApplyEndGearChange(logicalEnd, Car.EndGearStateKey.IsAirConnected, boolValue: false);
                }

                if (endGear.IsAnglecockOpen)
                {
                    car.ApplyEndGearChange(logicalEnd, Car.EndGearStateKey.Anglecock, 0f);
                }

                endGear.Coupler?.SetOpen(true);
            }
            catch
            {
            }
        }

        private sealed class SpawnReasonScope : IDisposable
        {
            private readonly VehicleSpawnReason _previous;

            public SpawnReasonScope(VehicleSpawnReason previous)
            {
                _previous = previous;
            }

            public void Dispose()
            {
                _currentSpawnReason = _previous;
            }
        }
    }
}
