using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;

namespace Ca.Jwsm.Railroader.Api.Core.Api
{
    public sealed class ApiHost : IApiHost
    {
        public ApiHost(
            ApiVersion version,
            IServiceRegistry services,
            ICapabilityService capabilities,
            IEventBus events,
            IDiagnosticsService diagnostics)
        {
            Version = version;
            Services = services;
            Capabilities = capabilities;
            Events = events;
            Diagnostics = diagnostics;
        }

        public ApiVersion Version { get; }

        public IServiceRegistry Services { get; }

        public ICapabilityService Capabilities { get; }

        public IEventBus Events { get; }

        public IDiagnosticsService Diagnostics { get; }
    }
}
