namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public enum ObserverFactKind
    {
        Unknown = 0,
        CommandIssued = 1,
        RouteBuildStarted = 2,
        RouteAccepted = 3,
        RouteBlocked = 4,
        WaypointNotice = 5,
        ArrivalConfirmed = 6,
        CouplingObserved = 7,
        AirConnected = 8,
        TopologyChanged = 9,
        ManualReviewRequested = 10,
        ManualContinueGranted = 11
    }
}
