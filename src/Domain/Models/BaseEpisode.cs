using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using FundingType = SFA.DAS.Learning.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;


public abstract class BaseEpisode<TEpisodeEntity, TEarningProfileDomainModel>: AggregateComponent where TEpisodeEntity : BaseEpisodeEntity
{
    protected readonly TEpisodeEntity _entity;
    protected int _ageAtStartOfApprenticeship;
    protected TEarningProfileDomainModel? _earningsProfile;
    public Guid EpisodeKey => _entity.Key;
    public long UKPRN => _entity.Ukprn;
    public long EmployerAccountId => _entity.EmployerAccountId;
    public string TrainingCode => _entity.TrainingCode;
    public FundingType FundingType => _entity.FundingType;
    public string LegalEntityName => _entity.LegalEntityName;
    public long? FundingEmployerAccountId => _entity.FundingEmployerAccountId;
    public TEarningProfileDomainModel? EarningsProfile => _earningsProfile;
    public int AgeAtStartOfApprenticeship => _ageAtStartOfApprenticeship;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;

    protected BaseEpisode(TEpisodeEntity model, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        _entity = model;
    }

    public void UpdateWithdrawalDate(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        _entity.WithdrawalDate = withdrawalDate;
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(DateTime? completionDate)
    {
        _entity.CompletionDate = completionDate;
    }
}
