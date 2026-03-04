using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System.Collections.ObjectModel;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;

public class Learning : AggregateRoot
{
    private LearningEntity _entity;
    private readonly List<ApprenticeshipEpisode> _apprenticeshipEpisodes;
    private readonly List<ShortCourseEpisode> _shortCourseEpisodes;

    public Guid ApprenticeshipKey => _entity.LearningKey;
    public long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public string Uln => _entity.Uln;
    public bool HasEHCP => _entity?.HasEHCP ?? false;
    public bool IsCareLeaver => _entity?.IsCareLeaver ?? false;
    public bool CareLeaverEmployerConsentGiven => _entity?.CareLeaverEmployerConsentGiven ?? false;
    public DateTime DateOfBirth => _entity.DateOfBirth;
    public IReadOnlyCollection<ApprenticeshipEpisode> ApprenticeshipEpisodes => new ReadOnlyCollection<ApprenticeshipEpisode>(_apprenticeshipEpisodes);
    public IReadOnlyCollection<ShortCourseEpisode> ShortCourseEpisodes => new ReadOnlyCollection<ShortCourseEpisode>(_shortCourseEpisodes);

    private Learning(LearningEntity entity)
    {
        _entity = entity;
        _apprenticeshipEpisodes = _entity.ApprenticeshipEpisodes.Select(this.GetApprenticeshipEpisodeFromEntity).ToList();
        _shortCourseEpisodes = _entity.ShortCourseEpisodes.Select(this.GetShortCourseEpisodeFromEntity).ToList();
    }

    public static Learning Get(LearningEntity entity)
    {
        return new Learning(entity);
    }

    public LearningEntity GetModel()
    {
        return _entity;
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

        _entity.HasEHCP = hasEHCP;
        _entity.IsCareLeaver = isCareLeaver;
        _entity.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
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
    public void UpdateEnglishAndMathsCourses(List<EnglishAndMaths.EnglishAndMaths> englishAndMathsCourses, ISystemClockService systemClock)
    {
        var currentEpisode = this.GetCurrentEpisode(systemClock);
        currentEpisode.UpdateEnglishAndMaths(englishAndMathsCourses, systemClock);
    }

    public void UpdateDateOfBirth(DateTime dateOfBirth)
    {
        _entity.DateOfBirth = dateOfBirth;
        foreach (var episode in ApprenticeshipEpisodes)
        {
            episode.UpdateAgeAtStart(dateOfBirth);
        }
    }

    public void UpdateUnapprovedShortCourseInformation(ShortCourseUpdateModel updateModel)
    {
        _entity.Uln = updateModel.Uln;
        var episode = _entity.ShortCourseEpisodes.Single();
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

#pragma warning disable CS8618
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
#pragma warning restore CS8618