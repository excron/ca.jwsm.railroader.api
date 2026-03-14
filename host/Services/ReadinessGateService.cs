using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Orders.Contracts;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ReadinessGateService : IReadinessGateService
    {
        private readonly List<IReadinessGate> _gates = new List<IReadinessGate>();
        private readonly object _sync = new object();

        public Result Register(IReadinessGate gate)
        {
            if (gate == null)
            {
                return Result.Failure("Readiness gate is required.");
            }

            lock (_sync)
            {
                foreach (var existingGate in _gates)
                {
                    if (string.Equals(existingGate.Name, gate.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result.Failure("A readiness gate with the same name is already registered.");
                    }
                }

                _gates.Add(gate);
            }

            return Result.Success();
        }

        public IEnumerable<ReadinessGate> Evaluate(OrderRequest request, ExecutionObservationState observationState)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var evaluations = new List<ReadinessGate>();

            lock (_sync)
            {
                foreach (var gate in _gates)
                {
                    evaluations.Add(gate.Evaluate(request, observationState));
                }
            }

            return evaluations;
        }
    }
}
