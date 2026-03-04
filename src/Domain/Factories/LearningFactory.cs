using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using FundingType = SFA.DAS.Learning.Types.FundingType;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class LearningFactory : ILearningFactory
{
    public Models.Learning CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum)
    {
        var model = new LearningEntity
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            LearningKey = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            ApprenticeshipEpisodes = new List<ApprenticeshipEpisodeEntity> { new ApprenticeshipEpisodeEntity(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode, fundingBandMaximum, null) },
            DateOfBirth = learningCreatedEvent.DateOfBirth
        };

        return Models.Learning.Get(model);
    }

    public Models.Learning GetExisting(LearningEntity model)
    {
        return Models.Learning.Get(model);
    }

    public Models.Learning CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest)
    {
        var model = new LearningEntity
        {
            LearningKey = commandRequest.LearningKey,
            DateOfBirth = commandRequest.Learner.DateOfBirth,
            Uln = commandRequest.Learner.Uln,
            ShortCourseEpisodes = new List<ShortCourseEpisodeEntity> {  new ShortCourseEpisodeEntity
            {
                Key = Guid.NewGuid(),
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
                CoursePrice = commandRequest.OnProgramme.TotalPrice                 
            } },
        };

        return Models.Learning.Get(model);
    }
}