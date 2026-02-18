using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Apprenticeship : AggregateRoot
{
    private Apprenticeship(LearningModel model)
    {
        _model = model;
        _episodes = _model.Episodes.Select(this.GetEpisodeFromModel).ToList();
    }

    private LearningModel _model;
    private readonly List<ApprenticeshipEpisode> _episodes;

    public Guid ApprenticeshipKey => _model.LearningKey;
    public long ApprovalsApprenticeshipId => _model.ApprovalsApprenticeshipId;
    public string Uln => _model.Uln;
    public bool HasEHCP => _model?.HasEHCP ?? false;
    public bool IsCareLeaver => _model?.IsCareLeaver ?? false;
    public bool CareLeaverEmployerConsentGiven => _model?.CareLeaverEmployerConsentGiven ?? false;
    public DateTime DateOfBirth => _model.DateOfBirth;

    public IReadOnlyCollection<ApprenticeshipEpisode> ApprenticeshipEpisodes => new ReadOnlyCollection<ApprenticeshipEpisode>(_episodes);
    
    public static Apprenticeship Get(LearningModel entity)
    {
        return new Apprenticeship(entity);
    }

    public LearningModel GetModel()
    {
        return _model;
    }

    public void Calculate(ISystemClockService systemClock, string calculationData, Guid? episodeKey = null)
    {
        ApprenticeshipEpisode episode;

        if(episodeKey.HasValue)
        {
            episode = this.GetEpisode(episodeKey.Value);
        }
        else
        {
            episode = this.GetCurrentEpisode(systemClock);
        }

        episode.CalculateOnProgram(this, systemClock, calculationData);
    }

    public void UpdateCareDetails(bool hasEHCP, bool isCareLeaver, bool careLeaverEmployerConsentGiven, ISystemClockService systemClock)
    {
        if(HasEHCP == hasEHCP && IsCareLeaver == isCareLeaver && CareLeaverEmployerConsentGiven == careLeaverEmployerConsentGiven)
        {
            return;
        }

        _model.HasEHCP = hasEHCP;
        _model.IsCareLeaver = isCareLeaver;
        _model.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
        var currentEpisode = this.GetCurrentEpisode(systemClock);
    }

    /// <summary>
    /// Adds additional earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Some earnings are generated separately using this endpoint, while others are handled as part of the normal process.
    /// Note, any existing additional payments of the type being added will be removed.
    /// </summary>
    /// <param name="additionalPayments"> The additional payments to be added.</param>
    /// <param name="systemClock"> The system clock service to be used for date calculations.</param>
    public void AddAdditionalEarnings(List<AdditionalPayment> additionalPayments, string additionalPaymentType, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.AddAdditionalEarnings(additionalPayments, additionalPaymentType, systemClock);
    }

    /// <summary>
    /// Adds maths and english course earnings to an apprenticeship that are not included in the standard earnings calculation process.
    /// Maths and English course earnings are generated separately using this endpoint.
    /// Note, any existing earnings for maths and english courses will be removed.
    /// </summary>
    public void UpdateMathsAndEnglishCourses(List<MathsAndEnglish> englishAndMathsCourses, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateEnglishAndMaths(englishAndMathsCourses, systemClock);
    }

    public void UpdateDateOfBirth(DateTime dateOfBirth)
    {
        _model.DateOfBirth = dateOfBirth;
        foreach (var episode in ApprenticeshipEpisodes)
        {
            episode.UpdateAgeAtStart(dateOfBirth);
        }
    }

    public void UpdateUnapprovedShortCourseInformation(ShortCourseUpdateModel updateModel)
    {
        _model.Uln = updateModel.Uln;
        _model.Episodes.Single().TrainingCode = updateModel.CourseCode;
        _model.Episodes.Single().EmployerAccountId = updateModel.EmployerId;
        _model.Episodes.Single().Ukprn = updateModel.Ukprn;
        _model.Episodes.Single().Prices.Single().StartDate = updateModel.StartDate;
        _model.Episodes.Single().WithdrawalDate = updateModel.WithdrawalDate;
        _model.Episodes.Single().CompletionDate = updateModel.CompletionDate;
        _model.Episodes.Single().Prices.Single().EndDate = updateModel.ExpectedEndDate;
        _model.Episodes.Single().Prices.Single().AgreedPrice = updateModel.TotalPrice;
    }
}

public class ShortCourseUpdateModel
{
    public string Uln { get; set; }
    public string CourseCode { get; set; }
    public long EmployerId { get; set; }
    public long Ukprn { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public decimal TotalPrice { get; set; }
}