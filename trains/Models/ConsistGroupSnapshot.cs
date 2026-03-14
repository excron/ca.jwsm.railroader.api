using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class ConsistGroupSnapshot
    {
        public ConsistGroupSnapshot(
            string id,
            string label,
            bool isLead,
            bool isOnline,
            IReadOnlyList<VehicleId> vehicleIds)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Group id is required.", nameof(id));
            }

            Id = id;
            Label = label;
            IsLead = isLead;
            IsOnline = isOnline;
            VehicleIds = vehicleIds ?? new VehicleId[0];
        }

        public string Id { get; }

        public string Label { get; }

        public bool IsLead { get; }

        public bool IsOnline { get; }

        public IReadOnlyList<VehicleId> VehicleIds { get; }
    }
}
