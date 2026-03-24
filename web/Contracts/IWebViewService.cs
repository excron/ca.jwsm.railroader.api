using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Web.Models;

namespace Ca.Jwsm.Railroader.Api.Web.Contracts
{
    public interface IWebViewService
    {
        WebRuntimeStatus GetStatus();

        Result<WebRailGraphSnapshot> GetRailGraph();

        Result<WebVehicleMapSnapshot> GetVehicles(WebVehicleFeedRequest request);

        Result<WebTrainPlacementResult> PlaceTrain(WebTrainPlacementRequest request);
    }
}
