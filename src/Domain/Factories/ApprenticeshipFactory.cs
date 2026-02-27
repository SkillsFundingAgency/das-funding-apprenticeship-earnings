using SFA.DAS.Learning.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using FundingType = SFA.DAS.Learning.Enums.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;

public class ApprenticeshipFactory : IApprenticeshipFactory
{
    public Apprenticeship.Apprenticeship CreateNew(LearningCreatedEvent learningCreatedEvent, int fundingBandMaximum)
    {
        var model = new LearningModel
        {
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            LearningKey = learningCreatedEvent.LearningKey,
            Uln = learningCreatedEvent.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(learningCreatedEvent.LearningKey, learningCreatedEvent.Episode, fundingBandMaximum, null) },
            DateOfBirth = learningCreatedEvent.DateOfBirth
        };

        return Apprenticeship.Apprenticeship.Get(model);
    }

    public Apprenticeship.Apprenticeship GetExisting(LearningModel model)
    {
        return Apprenticeship.Apprenticeship.Get(model);
    }

    public Apprenticeship.Apprenticeship CreateNewShortCourse(CreateUnapprovedShortCourseLearningRequest commandRequest)
    {
        var model = new LearningModel
        {
            LearningKey = commandRequest.LearningKey,
            DateOfBirth = commandRequest.Learner.DateOfBirth,
            Uln = commandRequest.Learner.Uln,
            Episodes = new List<EpisodeModel> { new EpisodeModel(commandRequest.LearningKey, new LearningEpisode
            {
                EmployerAccountId = commandRequest.OnProgramme.EmployerId,
                TrainingCode = commandRequest.OnProgramme.CourseCode,
                AgeAtStartOfLearning = GetAgeAtStartOfLearning(commandRequest.OnProgramme.StartDate, commandRequest.Learner.DateOfBirth),
                FundingBandMaximum = (int)Math.Ceiling(commandRequest.OnProgramme.TotalPrice), //todo maybe FBM becomes relevant in a future story?
                FundingType = FundingType.Levy,
                Key = Guid.NewGuid(),
                LegalEntityName = string.Empty,
                Prices = new List<LearningEpisodePrice>
                {
                    new LearningEpisodePrice
                    {
                        TotalPrice = commandRequest.OnProgramme.TotalPrice,
                        StartDate = commandRequest.OnProgramme.StartDate,
                        EndDate = commandRequest.OnProgramme.ExpectedEndDate,
                        Key = Guid.NewGuid()
                    }
                }
            }, (int)Math.Ceiling(commandRequest.OnProgramme.TotalPrice), commandRequest.OnProgramme.CompletionDate) }
        };

        return Apprenticeship.Apprenticeship.Get(model);
    }

    private int GetAgeAtStartOfLearning(DateTime startDate, DateTime dateOfBirth)
    {
        var age = startDate.Year - dateOfBirth.Year;

        if (startDate < dateOfBirth.AddYears(age)) age--;

        return age;
    }
}
