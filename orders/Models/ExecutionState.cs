namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public enum ExecutionState
    {
        Pending = 0,
        Accepted = 1,
        WaitingForObservation = 2,
        AwaitingManualContinue = 3,
        Running = 4,
        Completed = 5,
        Failed = 6,
        Cancelled = 7,
        Rejected = 8
    }
}
