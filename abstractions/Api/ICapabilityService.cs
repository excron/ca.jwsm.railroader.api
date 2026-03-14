using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Api
{
    public interface ICapabilityService
    {
        void SetCapability(CapabilityId capabilityId, bool enabled);

        bool HasCapability(CapabilityId capabilityId);

        IEnumerable<CapabilityId> GetCapabilities();
    }
}
