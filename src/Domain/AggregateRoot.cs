namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public abstract class AggregateRoot : AggregateComponent
{
    private readonly List<AggregateComponent> _children = new();
    public override Action<AggregateComponent> AddChildToRoot => AddChild;

    protected AggregateRoot():base(null)
    {
        
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

    private void AddChild(AggregateComponent child)
    {
        lock (_children)
        {
            _children.Add(child);
        }
    }
}

public abstract class AggregateComponent
{
    private readonly List<IDomainEvent> _events = new List<IDomainEvent>();
    public virtual Action<AggregateComponent> AddChildToRoot { get; }

    public AggregateComponent(Action<AggregateComponent>? addChildToRoot)
    {
        if (this is AggregateRoot)
        {
            AddChildToRoot = (AggregateComponent) =>
            {
                throw new InvalidOperationException("AggregateRoot should be overridden in the AggregateRoot component");
            };
            return;
        }


        if(addChildToRoot == null)
            throw new ArgumentNullException(nameof(addChildToRoot), "AddChildToRoot action must be provided for AggregateComponent");


        AddChildToRoot = addChildToRoot;
        AddChildToRoot(this);
    }

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
