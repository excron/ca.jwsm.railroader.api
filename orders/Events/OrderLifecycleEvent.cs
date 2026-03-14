using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Orders.Models;

namespace Ca.Jwsm.Railroader.Api.Orders.Events
{
    public sealed class OrderLifecycleEvent
    {
        public OrderLifecycleEvent(
            OrderCorrelationId correlationId,
            string subjectId,
            ExecutionState state,
            string message = null,
            IReadOnlyList<ObserverFact> facts = null,
            DateTimeOffset? timestamp = null)
        {
            CorrelationId = correlationId;
            SubjectId = subjectId;
            State = state;
            Message = message;
            Facts = facts ?? new ObserverFact[0];
            Timestamp = timestamp ?? DateTimeOffset.UtcNow;
        }

        public OrderCorrelationId CorrelationId { get; }

        public string SubjectId { get; }

        public ExecutionState State { get; }

        public string Message { get; }

        public IReadOnlyList<ObserverFact> Facts { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
