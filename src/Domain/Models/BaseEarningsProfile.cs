using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using UUIDNext;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;


public abstract class BaseEarningsProfile : AggregateComponent
{
    public abstract bool IsApproved { get; }

    protected BaseEarningsProfile(Action<AggregateComponent> addChildToRoot) : base(addChildToRoot) { }
}

public class BaseEarningsProfile<T> : BaseEarningsProfile where T : BaseEarningsProfileEntity, new()
{
    protected T Entity { get; set; }

    public Guid EarningsProfileId => Entity.EarningsProfileId;
    public decimal OnProgramTotal => Entity.OnProgramTotal;
    public decimal CompletionPayment => Entity.CompletionPayment;
    public Guid Version => Entity.Version;
    public override bool IsApproved => Entity.IsApproved;
    public string CalculationData => Entity.CalculationData;

    public BaseEarningsProfile(T entity, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        Entity = entity;
    }

    public BaseEarningsProfile(decimal onProgramTotal,
        decimal completionPayment,
        Guid episodeKey,
        bool isApproved,
        string calculationData,
        Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        var earningProfileId = Guid.NewGuid();

        Entity = new T();
        Entity.EarningsProfileId = earningProfileId;
        Entity.OnProgramTotal = onProgramTotal;
        Entity.CompletionPayment = completionPayment;
        Entity.EpisodeKey = episodeKey;
        Entity.Version = Uuid.NewDatabaseFriendly(Database.SqlServer);
        Entity.IsApproved = isApproved;
        Entity.CalculationData = calculationData;

    }

    public void Approve()
    {
        Entity.IsApproved = true;
    }

    public T GetModel()
    {
        return Entity;
    }
}