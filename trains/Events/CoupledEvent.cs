using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Trains.Events
{
    public sealed class CoupledEvent
    {
        public CoupledEvent(CouplerEndId first, CouplerEndId second)
        {
            First = first;
            Second = second;
        }

        public CouplerEndId First { get; }

        public CouplerEndId Second { get; }
    }
}
