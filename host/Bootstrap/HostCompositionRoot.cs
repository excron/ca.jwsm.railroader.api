using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics;
using Ca.Jwsm.Railroader.Api.Abstractions.Events;
using Ca.Jwsm.Railroader.Api.Core.Api;
using Ca.Jwsm.Railroader.Api.Core.Diagnostics;
using Ca.Jwsm.Railroader.Api.Core.Events;
using Ca.Jwsm.Railroader.Api.Host.Services;
using Ca.Jwsm.Railroader.Api.Orders.Contracts;
using Ca.Jwsm.Railroader.Api.Persistence.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Contracts;

namespace Ca.Jwsm.Railroader.Api.Host.Bootstrap
{
    public sealed class HostCompositionRoot
    {
        public IApiHost Build()
        {
            var services = new ServiceRegistry();
            var capabilities = new CapabilityService();
            var events = new EventBus();
            var diagnostics = new DiagnosticsService();
            var aeBridge = new AeBridgeService();
            var executionObserver = new ExecutionObserverService(events);
            var readinessGates = new ReadinessGateService();
            var hud = new HudService();
            var hudContext = new HudContextService();
            var controlContext = new ControlContextService();
            var controlRequests = new ControlRequestService();
            var consistTopology = new ConsistTopologyService();
            var saveContext = new SaveContextService(events);
            var modDataStore = new ModDataStore(saveContext);
            var trainIntegration = new TrainIntegrationService(events);
            var couplerInteractions = new CouplerInteractionService();

            services.Register<IServiceRegistry>(services);
            services.Register<ICapabilityService>(capabilities);
            services.Register<IEventBus>(events);
            services.Register<IDiagnosticsService>(diagnostics);
            services.Register<IAEBridgeService>(aeBridge);
            services.Register<IExecutionObserverService>(executionObserver);
            services.Register<IReadinessGateService>(readinessGates);
            services.Register<IHudService>(hud);
            services.Register<IHudContextService>(hudContext);
            services.Register<IControlContextService>(controlContext);
            services.Register<IControlRequestService>(controlRequests);
            services.Register<IConsistTopologyService>(consistTopology);
            services.Register<ICouplerInteractionService>(couplerInteractions);
            services.Register<ISaveContextService>(saveContext);
            services.Register<ISaveLifecycleService>(saveContext);
            services.Register<IModDataStore>(modDataStore);

            var host = new ApiHost(
                new ApiVersion(1, 0, 0),
                services,
                capabilities,
                events,
                diagnostics);

            services.Register<IApiHost>(host);
            Host.Patches.SaveLifecycle.Service = saveContext;
            Host.Patches.TrainIntegrationState.Service = trainIntegration;
            Host.Patches.CouplerInteractionState.Service = couplerInteractions;

            return host;
        }
    }
}
