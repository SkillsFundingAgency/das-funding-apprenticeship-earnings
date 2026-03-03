using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public class Learning : AggregateRoot
{
    private Learning(LearningEntity model)
    {
        _model = model;
        _apprenticeshipEpisodes = _model.ApprenticeshipEpisodes.Select(this.GetApprenticeshipEpisodeFromModel).ToList();
        _shortCourseEpisodes = _model.ShortCourseEpisodes.Select(this.GetShortCourseEpisodeFromModel).ToList();
    }

    private LearningEntity _model;
    private readonly List<ApprenticeshipEpisode> _apprenticeshipEpisodes;
    private readonly List<ShortCourseEpisode> _shortCourseEpisodes;

    public Guid ApprenticeshipKey => _model.LearningKey;
    public long ApprovalsApprenticeshipId => _model.ApprovalsApprenticeshipId;
    public string Uln => _model.Uln;
    public bool HasEHCP => _model?.HasEHCP ?? false;
    public bool IsCareLeaver => _model?.IsCareLeaver ?? false;
    public bool CareLeaverEmployerConsentGiven => _model?.CareLeaverEmployerConsentGiven ?? false;
    public DateTime DateOfBirth => _model.DateOfBirth;

    public IReadOnlyCollection<ApprenticeshipEpisode> ApprenticeshipEpisodes => new ReadOnlyCollection<ApprenticeshipEpisode>(_apprenticeshipEpisodes);
    public IReadOnlyCollection<ShortCourseEpisode> ShortCourseEpisodes => new ReadOnlyCollection<ShortCourseEpisode>(_shortCourseEpisodes);

    public static Learning Get(LearningEntity entity)
    {
        return new Learning(entity);
    }

    public LearningEntity GetModel()
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
        var episode = _model.ShortCourseEpisodes.Single();
        episode.TrainingCode = updateModel.CourseCode;
        episode.EmployerAccountId = updateModel.EmployerId;
        episode.Ukprn = updateModel.Ukprn;
        episode.StartDate = updateModel.StartDate;
        episode.WithdrawalDate = updateModel.WithdrawalDate;
        episode.CompletionDate = updateModel.CompletionDate;
        episode.EndDate = updateModel.ExpectedEndDate;
        episode.CoursePrice = updateModel.TotalPrice;
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