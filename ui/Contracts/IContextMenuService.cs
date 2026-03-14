using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Ui.Contracts
{
    public interface IContextMenuService
    {
        Result Register(ContextMenuContribution contribution);
    }
}
