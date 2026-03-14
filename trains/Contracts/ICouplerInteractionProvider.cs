using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Contracts
{
    public interface ICouplerInteractionProvider
    {
        void PopulateTooltip(CouplerInteractionContext context, CouplerTooltipContent tooltip);

        void PopulateMenu(CouplerInteractionContext context, CouplerMenuContent menu);
    }
}
