namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public abstract class AggregateRoot : AggregateComponent
{
    private readonly List<AggregateComponent> _children = new();
    public override Action<AggregateComponent> AddChildToRoot => AddChild;

    protected AggregateRoot() : base(null)
    {

    }

    public override IEnumerable<object> FlushEvents()
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

    public override bool HasEvent<T>()
    {
        if (base.HasEvent<T>())
            return true;

        lock (_children)
        {
            return _children.Any(child => child.HasEvent<T>());
        }
    }
}

public abstract class AggregateComponent
{
    private readonly List<object> _events = new List<object>();
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

    protected void AddEvent(object @event)
    {
        lock (_events)
        {
            _events.Add(@event);
        }
    }

    public virtual IEnumerable<object> FlushEvents()
    {
        lock (_events)
        {
            var events = _events.ToArray();
            _events.Clear();
            return events;
        }
    }

    public virtual bool HasEvent<T>()
    {
        return _events.Any(x => x is T);
    }

    public void PurgeEventsOfType<T>()
    {
        _events.RemoveAll(e => e is T);
    }
}