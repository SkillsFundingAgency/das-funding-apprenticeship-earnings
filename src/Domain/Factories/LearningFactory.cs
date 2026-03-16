using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
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
            Episodes = new List<ShortCourseEpisodeEntity> {  new ShortCourseEpisodeEntity
            {
                Key = commandRequest.EpisodeKey,
                LearningKey = commandRequest.LearningKey,
                Ukprn = commandRequest.OnProgramme.Ukprn,
                EmployerAccountId = commandRequest.OnProgramme.EmployerId,
                FundingType = FundingType.Levy,
                FundingEmployerAccountId = null,
                LegalEntityName = string.Empty,
                TrainingCode = commandRequest.OnProgramme.CourseCode,
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