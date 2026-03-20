using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Ui.Contracts
{
    public interface IOverlayTextService
    {
        Result Register(OverlayTextPanelDescriptor descriptor);

        Result Unregister(string id);

        IReadOnlyList<OverlayTextPanelDescriptor> GetDescriptors();
    }
}
