using System.Collections.Generic;
using System.Linq;
using Ca.Jwsm.Railroader.Api.Abstractions.Api;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;

namespace Ca.Jwsm.Railroader.Api.Core.Api
{
    public sealed class CapabilityService : ICapabilityService
    {
        private readonly HashSet<CapabilityId> _capabilities = new HashSet<CapabilityId>();

        public void SetCapability(CapabilityId capabilityId, bool enabled)
        {
            if (enabled)
            {
                _capabilities.Add(capabilityId);
                return;
            }

            _capabilities.Remove(capabilityId);
        }

        public bool HasCapability(CapabilityId capabilityId)
        {
            return _capabilities.Contains(capabilityId);
        }

        public IEnumerable<CapabilityId> GetCapabilities()
        {
            return _capabilities.ToArray();
        }
    }
}
