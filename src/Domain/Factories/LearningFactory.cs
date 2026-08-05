using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;
using FundingType = SFA.DAS.Learning.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class LearningFactory : ILearningFactory
{
    public ApprenticeshipLearning CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum)
    {
        var model = new ApprenticeshipLearningEntity
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            LearningKey = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            Episodes = new List<ApprenticeshipEpisodeEntity> { new ApprenticeshipEpisodeEntity(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode, fundingBandMaximum, null) },
            DateOfBirth = learningCreatedEvent.DateOfBirth
        };

        return ApprenticeshipLearning.Get(model);
    }

    public ApprenticeshipLearning GetExistingApprenticeship(ApprenticeshipLearningEntity model)
    {
        return ApprenticeshipLearning.Get(model);
    }

    public ApprenticeshipLearning CreateNewUnapprovedApprenticeship(CreateUnapprovedApprenticeshipLearningRequest request, int fundingBandMaximum)
    {
        if (request.Prices.Count == 0)
        {
            throw new InvalidOperationException($"No prices were supplied for episode {request.EpisodeKey}.");
        }

        var episodePrices = request.Prices
            .Select(price => new ApprenticeshipEpisodePriceEntity(request.EpisodeKey, price))
            .ToList();

        var periodsInLearning = request.PeriodsInLearning.Any()
            ? request.PeriodsInLearning.Select(x => new ApprenticeshipPeriodInLearningEntity
            {
                Key = Guid.NewGuid(),
                EpisodeKey = request.EpisodeKey,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                OriginalExpectedEndDate = x.OriginalExpectedEndDate
            }).ToList()
            : episodePrices.Select(x => x.ToSinglePeriodInLearning()).ToList();

        var episode = new ApprenticeshipEpisodeEntity
        {
            Key = request.EpisodeKey,
            LearningKey = request.LearningKey,
            Ukprn = request.OnProgramme.Ukprn,
            EmployerAccountId = request.OnProgramme.EmployerAccountId,
            FundingEmployerAccountId = request.OnProgramme.FundingEmployerAccountId,
            FundingType = request.OnProgramme.FundingType,
            TrainingCode = request.OnProgramme.TrainingCode,
            LegalEntityName = request.OnProgramme.LegalEntityName,
            FundingBandMaximum = fundingBandMaximum,
            CompletionDate = request.CompletionDate,
            WithdrawalDate = request.WithdrawalDate,
            AchievementDate = request.AchievementDate,
            PauseDate = request.PauseDate,
            Prices = episodePrices,
            PeriodsInLearning = periodsInLearning
        };

        var model = new ApprenticeshipLearningEntity
        {
            LearningKey = request.LearningKey,
            ApprovalsApprenticeshipId = request.ApprovalsApprenticeshipId,
            Uln = request.Learner.Uln,
            DateOfBirth = request.Learner.DateOfBirth,
            HasEHCP = request.Learner.Care.HasEHCP,
            IsCareLeaver = request.Learner.Care.IsCareLeaver,
            CareLeaverEmployerConsentGiven = request.Learner.Care.CareLeaverEmployerConsentGiven,
            Episodes = new List<ApprenticeshipEpisodeEntity> { episode }
        };

        return ApprenticeshipLearning.Get(model);
    }

    public ShortCourseLearning GetExistingShortCourse(ShortCourseLearningEntity model)
    {
        return ShortCourseLearning.Get(model);
    }

    public ShortCourseLearning CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest)
    {
        var model = new ShortCourseLearningEntity
        {
            LearningKey = commandRequest.LearningKey,
            DateOfBirth = commandRequest.Learner.DateOfBirth,
            Uln = commandRequest.Learner.Uln,
            TrainingCode = commandRequest.OnProgramme.CourseCode,
            Episodes = new List<ShortCourseEpisodeEntity> {  new ShortCourseEpisodeEntity
            {
                Key = commandRequest.EpisodeKey,
                LearningKey = commandRequest.LearningKey,
                Ukprn = commandRequest.OnProgramme.Ukprn,
                FundingType = FundingType.Levy,
                EmployerType = EmployerType.NonLevy,
                CompletionDate = commandRequest.OnProgramme.CompletionDate,
                WithdrawalDate = commandRequest.OnProgramme.WithdrawalDate,
                StartDate = commandRequest.OnProgramme.StartDate,
                EndDate = commandRequest.OnProgramme.ExpectedEndDate,
                CoursePrice = commandRequest.OnProgramme.TotalPrice,
                Milestones = commandRequest.OnProgramme.Milestones.ToMilestoneFlags()
            } },
        };

        return ShortCourseLearning.Get(model);
    }
}