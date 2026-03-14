using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Orders.Contracts
{
    public interface IAEBridgeService
    {
        Result<OrderCorrelationId> Submit(OrderRequest request);

        Result Continue(OrderCorrelationId correlationId);
    }
}
