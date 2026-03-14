using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Orders.Contracts
{
    public interface IReadinessGate
    {
        string Name { get; }

        ReadinessGate Evaluate(OrderRequest request, ExecutionObservationState observationState);
    }
}
