﻿namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain
{
    public abstract class AggregateRoot
    {
        private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

        protected void AddEvent(IDomainEvent @event)
        {
            lock (_events)
            {
                _events.Add(@event);
            }
        }

        public IEnumerable<IDomainEvent> FlushEvents()
        {
            lock (_events)
            {
                var events = _events.ToArray();
                _events.Clear();
                return events;
            }
        }

        public bool HasEvent<T>()
        {
            return _events.Any(x => x is T);
        }
    }
}
