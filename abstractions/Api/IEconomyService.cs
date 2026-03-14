using Ca.Jwsm.Railroader.Api.Abstractions.Common;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Api
{
    public interface IEconomyService
    {
        int CurrentBalance { get; }

        Result Charge(int amount, string reason);
    }
}
