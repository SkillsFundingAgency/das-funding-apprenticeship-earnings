using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;

public static class LearningCreatedEventExtensions
{
    public static CreateUnapprovedApprenticeshipLearningRequest ToCreateUnapprovedApprenticeshipLearningRequest(
        this LearningCreatedEvent learningCreatedEvent, int? fundingBandMaximum)
    {
        return new CreateUnapprovedApprenticeshipLearningRequest
        {
            IsNewApprenticeshipLearner = true,
            LearningKey = learningCreatedEvent.LearningKey,
            EpisodeKey = learningCreatedEvent.Episode.Key,
            ApprovalsApprenticeshipId = learningCreatedEvent.ApprovalsApprenticeshipId,
            Learner = new DraftApprenticeshipLearner
            {
                DateOfBirth = learningCreatedEvent.DateOfBirth,
                Uln = learningCreatedEvent.Uln,
                Care = new DraftCare()
            },
            OnProgramme = new DraftApprenticeshipOnProgramme
            {
                TrainingCode = learningCreatedEvent.Episode.TrainingCode,
                Ukprn = learningCreatedEvent.Episode.Ukprn,
                EmployerAccountId = learningCreatedEvent.Episode.EmployerAccountId,
                FundingEmployerAccountId = learningCreatedEvent.Episode.FundingEmployerAccountId,
                LegalEntityName = learningCreatedEvent.Episode.LegalEntityName,
                EmployerType = learningCreatedEvent.Episode.EmployerType.ToEmployerType(),
                FundingBandMaximum = fundingBandMaximum
            },
            Prices = learningCreatedEvent.Episode.Prices,
            PeriodsInLearning = learningCreatedEvent.Episode.Prices
                .Select(price => new ApprenticeshipPeriodInLearningItem
                {
                    StartDate = price.StartDate,
                    EndDate = null,
                    OriginalExpectedEndDate = price.EndDate
                })
                .ToList()
        };
    }
}
