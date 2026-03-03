using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using FundingType = SFA.DAS.Learning.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;


public abstract class BaseEpisode<TEpisodeEntity, TEarningProfileDomainModel>: AggregateComponent where TEpisodeEntity : BaseEpisodeEntity
{
    protected readonly TEpisodeEntity _model;
    private readonly DateTime _dateOfBirth;
    private int _ageAtStartOfApprenticeship;

    protected BaseEpisode(TEpisodeEntity model, DateTime dateOfBirth, Action<AggregateComponent> addChildToRoot) : base(addChildToRoot)
    {
        _model = model;
        _dateOfBirth = dateOfBirth;
    }

    protected TEarningProfileDomainModel? _earningsProfile;


    public Guid EpisodeKey => _model.Key;
    public long UKPRN => _model.Ukprn;
    public long EmployerAccountId => _model.EmployerAccountId;
    public string TrainingCode => _model.TrainingCode;
    public FundingType FundingType => _model.FundingType;
    public string LegalEntityName => _model.LegalEntityName;
    public long? FundingEmployerAccountId => _model.FundingEmployerAccountId;
    public TEarningProfileDomainModel? EarningsProfile => _earningsProfile;
    public int AgeAtStartOfApprenticeship => _ageAtStartOfApprenticeship;
    public DateTime? CompletionDate => _model.CompletionDate;
    public DateTime? WithdrawalDate => _model.WithdrawalDate;
    public DateTime? PauseDate => _model.PauseDate;
    public decimal FundingBandMaximum => _model.FundingBandMaximum;




    public void UpdateWithdrawalDate(DateTime? withdrawalDate, ISystemClockService systemClock)
    {
        _model.WithdrawalDate = withdrawalDate;
    }

    /// <summary>
    /// Updates the completion date and earnings profile accordingly with the completion instalment and balanced instalments if necessary.
    /// </summary>
    public void UpdateCompletion(DateTime? completionDate)
    {
        _model.CompletionDate = completionDate;
    }

    public void UpdateFundingBandMaximum(int fundingBandMaximum)
    {
        _model.FundingBandMaximum = fundingBandMaximum;
    }

    public void UpdateAgeAtStart(DateTime startDate)
    {
        _ageAtStartOfApprenticeship = _dateOfBirth.CalculateAgeAtDate(startDate);
    }
}
