using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Contracts
{
    public interface IWebViewService
    {
        WebRuntimeStatus GetStatus();

        Result<WebRailGraphSnapshot> GetRailGraph();

        Result<WebVehicleMapSnapshot> GetVehicles(WebVehicleFeedRequest request);

        Result<WebInfrastructureSnapshot> GetInfrastructure();

        Result<WebVirtualClientContextSnapshot> GetVirtualClientContext(string steamId);

        Result<WebLocomotiveControlSnapshot> GetLocomotiveControls(string vehicleId);

        Result<WebLocomotiveControlResult> ApplyLocomotiveControl(string steamId, WebLocomotiveControlRequest request);

        Result<WebLocomotiveModeResult> ApplyLocomotiveMode(string steamId, WebLocomotiveModeRequest request);

        Result<WebLocomotiveAutoEngineerResult> ApplyLocomotiveAutoEngineer(string steamId, WebLocomotiveAutoEngineerRequest request);

        Result<WebSwitchToggleResult> ToggleSwitch(string steamId, WebSwitchToggleRequest request);

        Result<WebTrainPlacementResult> PlaceTrain(WebTrainPlacementRequest request);
    }
}
