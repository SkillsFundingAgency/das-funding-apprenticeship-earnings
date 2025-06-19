namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public abstract class AggregateRoot : AggregateComponent
{
    private readonly List<AggregateComponent> _children = new();

    internal void AddChild(AggregateComponent child)
    {
        lock (_children)
        {
            _children.Add(child);
        }
    }

    public override IEnumerable<IDomainEvent> FlushEvents()
    {
        var allEvents = base.FlushEvents().ToList();

        lock (_children)
        {
            foreach (var child in _children)
            {
               allEvents.AddRange(child.FlushEvents());
            }
        }

        return allEvents;
    }
}

public abstract class AggregateComponent
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();

    protected void AddEvent(IDomainEvent @event)
    {
        lock (_events)
        {
            _events.Add(@event);
        }
    }

    public virtual IEnumerable<IDomainEvent> FlushEvents()
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
