using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Api
{
    public interface IApiHost
    {
        ApiVersion Version { get; }

        IServiceRegistry Services { get; }

        ICapabilityService Capabilities { get; }

        IEventBus Events { get; }

        IDiagnosticsService Diagnostics { get; }
    }
}
