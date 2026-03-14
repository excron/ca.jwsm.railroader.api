using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Orders.Models
{
    public sealed class ExecutionObservationState
    {
        public ExecutionObservationState(
            string subjectId,
            int commandSequence = 0,
            int routeSequence = 0,
            int noticeSequence = 0,
            bool isRouteBuildInProgress = false,
            bool lastRouteAccepted = false,
            bool lastRouteBlocked = false,
            string lastMessage = null,
            string lastReason = null,
            DateTimeOffset? lastObservedAt = null,
            IReadOnlyList<ObserverFact> recentFacts = null)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
            {
                throw new ArgumentException("Subject id is required.", nameof(subjectId));
            }

            SubjectId = subjectId;
            CommandSequence = commandSequence;
            RouteSequence = routeSequence;
            NoticeSequence = noticeSequence;
            IsRouteBuildInProgress = isRouteBuildInProgress;
            LastRouteAccepted = lastRouteAccepted;
            LastRouteBlocked = lastRouteBlocked;
            LastMessage = lastMessage;
            LastReason = lastReason;
            LastObservedAt = lastObservedAt ?? DateTimeOffset.UtcNow;
            RecentFacts = recentFacts ?? new ObserverFact[0];
        }

        public string SubjectId { get; }

        public int CommandSequence { get; }

        public int RouteSequence { get; }

        public int NoticeSequence { get; }

        public bool IsRouteBuildInProgress { get; }

        public bool LastRouteAccepted { get; }

        public bool LastRouteBlocked { get; }

        public string LastMessage { get; }

        public string LastReason { get; }

        public DateTimeOffset LastObservedAt { get; }

        public IReadOnlyList<ObserverFact> RecentFacts { get; }
    }
}
