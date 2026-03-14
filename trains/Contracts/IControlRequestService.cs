using System;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface IControlRequestService
    {
        Result RegisterInterceptor(string id, Func<ControlRequest, ControlRequestResult> interceptor);

        ControlRequestResult Evaluate(ControlRequest request);
    }
}
