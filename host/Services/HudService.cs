using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Ui.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class HudService : IHudService
    {
        private readonly Dictionary<string, HudWidgetDescriptor> _descriptors = new Dictionary<string, HudWidgetDescriptor>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public Result Register(HudWidgetDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return Result.Failure("HUD widget descriptor is required.");
            }

            lock (_sync)
            {
                if (_descriptors.ContainsKey(descriptor.Id))
                {
                    return Result.Failure("A HUD widget with the same id is already registered.");
                }

                _descriptors.Add(descriptor.Id, descriptor);
            }

            return Result.Success();
        }
    }
}
