using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public sealed class OrderRequest
    {
        public OrderRequest(
            string orderType,
            string subjectId,
            string description = null,
            IReadOnlyDictionary<string, string> parameters = null)
        {
            if (string.IsNullOrWhiteSpace(orderType))
            {
                throw new ArgumentException("Order type is required.", nameof(orderType));
            }

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                throw new ArgumentException("Subject id is required.", nameof(subjectId));
            }

            OrderType = orderType;
            SubjectId = subjectId;
            Description = description;
            Parameters = parameters ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }

        public string OrderType { get; }

        public string SubjectId { get; }

        public string Description { get; }

        public IReadOnlyDictionary<string, string> Parameters { get; }

        public bool TryGetParameter(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return Parameters.TryGetValue(key, out value);
        }
    }
}
