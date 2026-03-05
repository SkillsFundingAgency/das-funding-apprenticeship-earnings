using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipLearning : BaseLearning<ApprenticeshipLearningEntity, ApprenticeshipEpisode>
{
    private ApprenticeshipLearning(ApprenticeshipLearningEntity entity) : base(entity)
    {
        _episodes = _entity.Episodes.Select(this.GetApprenticeshipEpisodeFromEntity).ToList();
    }

    public static ApprenticeshipLearning Get(ApprenticeshipLearningEntity entity)
    {
        return new ApprenticeshipLearning(entity);
    }

    public void Calculate(ISystemClockService systemClock, string calculationData, Guid? episodeKey = null)
    {
        ApprenticeshipEpisode episode;

        if (episodeKey.HasValue)
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
        if (HasEHCP == hasEHCP && IsCareLeaver == isCareLeaver && CareLeaverEmployerConsentGiven == careLeaverEmployerConsentGiven)
        {
            return;
        }

        _entity.HasEHCP = hasEHCP;
        _entity.IsCareLeaver = isCareLeaver;
        _entity.CareLeaverEmployerConsentGiven = careLeaverEmployerConsentGiven;
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
        foreach (var episode in Episodes)
        {
            episode.UpdateAgeAtStart(dateOfBirth);
        }
    }
}
