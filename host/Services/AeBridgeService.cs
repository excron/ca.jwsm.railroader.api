using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Orders.Contracts;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class AeBridgeService : IAEBridgeService
    {
        public Result<OrderCorrelationId> Submit(OrderRequest request)
        {
            return Result<OrderCorrelationId>.Failure("Host AE integration is not implemented yet.");
        }

        public Result Continue(OrderCorrelationId correlationId)
        {
            return Result.Failure("Host AE integration is not implemented yet.");
        }
    }
}
