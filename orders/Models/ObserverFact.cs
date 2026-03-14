using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public sealed class ObserverFact
    {
        public ObserverFact(
            ObserverFactKind kind,
            string subjectId,
            long sequence,
            DateTimeOffset timestamp,
            string message = null,
            ObserverFactSource source = ObserverFactSource.Unknown,
            IReadOnlyDictionary<string, string> attributes = null)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
            {
                throw new ArgumentException("Subject id is required.", nameof(subjectId));
            }

            Kind = kind;
            SubjectId = subjectId;
            Sequence = sequence;
            Timestamp = timestamp;
            Message = message;
            Source = source;
            Attributes = attributes ?? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }

        public ObserverFactKind Kind { get; }

        public string SubjectId { get; }

        public long Sequence { get; }

        public string Message { get; }

        public ObserverFactSource Source { get; }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        public DateTimeOffset Timestamp { get; }
    }
}
