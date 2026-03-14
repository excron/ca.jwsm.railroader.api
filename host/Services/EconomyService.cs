using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Game.State;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class EconomyService : IEconomyService
    {
        public int CurrentBalance
        {
            get
            {
                try
                {
                    return StateManager.Shared?.Balance ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public Result Charge(int amount, string reason)
        {
            if (amount <= 0)
            {
                return Result.Success();
            }

            try
            {
                var stateManager = StateManager.Shared;
                if (stateManager == null)
                {
                    return Result.Failure("Economy state is unavailable.");
                }

                if (stateManager.Balance < amount)
                {
                    return Result.Failure("Insufficient funds.");
                }

                stateManager.ApplyToBalance(-amount, Ledger.Category.RepairSupplies, null, reason ?? "Charge");
                return Result.Success();
            }
            catch
            {
                return Result.Failure("Failed to apply the requested charge.");
            }
        }
    }
}
