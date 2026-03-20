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
using Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts;

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
            var economy = new EconomyService();
            var aeBridge = new AeBridgeService();
            var executionObserver = new ExecutionObserverService(events);
            var readinessGates = new ReadinessGateService();
            var hud = new HudService();
            var hudContext = new HudContextService();
            var notifications = new NotificationService();
            var overlayText = new OverlayTextService();
            var controlContext = new ControlContextService();
            var controlRequests = new ControlRequestService();
            var consistTopology = new ConsistTopologyService();
            var trains = new TrainService();
            var saveContext = new SaveContextService(events);
            var modDataStore = new ModDataStore(saveContext);
            var trainIntegration = new TrainIntegrationService(events);
            var couplerInteractions = new CouplerInteractionService();
            var wearFeatures = new WearFeatureService(events);
            var worldLayout = new WorldLayoutService();
            var worldAssetStores = new WorldAssetStoreService();

            services.Register<IServiceRegistry>(services);
            services.Register<ICapabilityService>(capabilities);
            services.Register<IEventBus>(events);
            services.Register<IDiagnosticsService>(diagnostics);
            services.Register<IEconomyService>(economy);
            services.Register<IAEBridgeService>(aeBridge);
            services.Register<IExecutionObserverService>(executionObserver);
            services.Register<IReadinessGateService>(readinessGates);
            services.Register<IHudService>(hud);
            services.Register<IHudContextService>(hudContext);
            services.Register<INotificationService>(notifications);
            services.Register<IOverlayTextService>(overlayText);
            services.Register<IControlContextService>(controlContext);
            services.Register<IControlRequestService>(controlRequests);
            services.Register<IConsistTopologyService>(consistTopology);
            services.Register<ITrainService>(trains);
            services.Register<IConsistService>(trains);
            services.Register<ICouplerInteractionService>(couplerInteractions);
            services.Register<WearFeatureService>(wearFeatures);
            services.Register<IWearFeatureService>(wearFeatures);
            services.Register<ISaveContextService>(saveContext);
            services.Register<ISaveLifecycleService>(saveContext);
            services.Register<IModDataStore>(modDataStore);
            services.Register<IWorldLayoutService>(worldLayout);
            services.Register<IWorldAssetStoreService>(worldAssetStores);

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
            Host.Patches.WorldLifecycleState.Service = worldLayout;
            Host.Patches.WorldAssetStoreState.Service = worldAssetStores;

            return host;
        }
    }
}
