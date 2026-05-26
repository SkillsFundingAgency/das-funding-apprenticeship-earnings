using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using FundingType = SFA.DAS.Learning.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public abstract class BaseEpisode : AggregateComponent
{
    protected BaseEpisode(Action<AggregateComponent> addChildToRoot) : base(addChildToRoot) { }

    public abstract Guid EpisodeKey { get; }
    public abstract BaseEarningsProfile? EarningsProfile { get; }
    public abstract void Approve();
}

public abstract class BaseEpisode<TEpisodeEntity, TEarningProfileDomainModel> : BaseEpisode
    where TEpisodeEntity : BaseEpisodeEntity
    where TEarningProfileDomainModel : BaseEarningsProfile
{
    protected readonly TEpisodeEntity _entity;
    protected int _ageAtStartOfApprenticeship;
    protected TEarningProfileDomainModel? _earningsProfile;
    public override Guid EpisodeKey => _entity.Key;
    public long UKPRN => _entity.Ukprn;
    public string TrainingCode => _entity.TrainingCode;
    public FundingType FundingType => _entity.FundingType;
    public override TEarningProfileDomainModel? EarningsProfile => _earningsProfile;
    public int AgeAtStartOfApprenticeship => _ageAtStartOfApprenticeship;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public DateTime? AchievementDate => _entity.AchievementDate;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;

    protected BaseEpisode(TEpisodeEntity model, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        _entity = model;
    }

    public void UpdateWithdrawalDate(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        _entity.WithdrawalDate = withdrawalDate;
    }

    public void UpdateAchievementDate(DateTime? achievementDate)
    {
        _entity.AchievementDate = achievementDate;
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(DateTime? completionDate)
    {
        _entity.CompletionDate = completionDate;
    }
}
