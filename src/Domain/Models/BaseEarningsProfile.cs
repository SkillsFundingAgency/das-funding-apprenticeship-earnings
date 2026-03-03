using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;


public class BaseEarningsProfile<T> : AggregateComponent where T : BaseEarningsProfileEntity, new()
{
    protected T Entity { get; set; }

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
        Entity.Version = Guid.NewGuid();
        Entity.IsApproved = isApproved;
        Entity.CalculationData = calculationData;

    }

    public Guid EarningsProfileId => Entity.EarningsProfileId;
    public decimal OnProgramTotal => Entity.OnProgramTotal;
    public decimal CompletionPayment => Entity.CompletionPayment;
    public Guid Version => Entity.Version;
    public bool IsApproved => Entity.IsApproved;
    public string CalculationData => Entity.CalculationData;

    public T GetModel()
    {
        return Entity;
    }
}