using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Orders.Contracts
{
    public interface IReadinessGateService
    {
        Result Register(IReadinessGate gate);

        IEnumerable<ReadinessGate> Evaluate(OrderRequest request, ExecutionObservationState observationState);
    }
}
