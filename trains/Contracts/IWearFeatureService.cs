namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface IWearFeatureService
    {
        bool IsWearAndTearEnabled { get; }

        float WearMultiplier { get; }

        int OverhaulMiles { get; }
    }
}
