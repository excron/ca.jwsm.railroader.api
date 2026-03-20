using System;
using System.Collections.Generic;
using System.Linq;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Ui.Contracts;
using Ca.Jwsm.Railroader.Api.Ui.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class OverlayTextService : IOverlayTextService
    {
        private readonly Dictionary<string, OverlayTextPanelDescriptor> _descriptors = new Dictionary<string, OverlayTextPanelDescriptor>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public Result Register(OverlayTextPanelDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return Result.Failure("Overlay text panel descriptor is required.");
            }

            if (string.IsNullOrWhiteSpace(descriptor.Id))
            {
                return Result.Failure("Overlay text panel id is required.");
            }

            if (descriptor.StateProvider == null)
            {
                return Result.Failure("Overlay text panel state provider is required.");
            }

            lock (_sync)
            {
                if (_descriptors.ContainsKey(descriptor.Id))
                {
                    return Result.Failure("An overlay text panel with the same id is already registered.");
                }

                _descriptors.Add(descriptor.Id, descriptor);
            }

            return Result.Success();
        }

        public Result Unregister(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result.Failure("Overlay text panel id is required.");
            }

            lock (_sync)
            {
                return _descriptors.Remove(id)
                    ? Result.Success()
                    : Result.Failure("Overlay text panel id was not registered.");
            }
        }

        public IReadOnlyList<OverlayTextPanelDescriptor> GetDescriptors()
        {
            lock (_sync)
            {
                return _descriptors.Values
                    .OrderByDescending(d => d.Priority)
                    .ThenBy(d => d.Id, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }
    }
}
