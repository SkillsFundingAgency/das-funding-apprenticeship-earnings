using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UnapprovedApprenticeshipSetupModel
{
    public int? Age { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? TotalPrice { get; set; }
}

public static class UnapprovedApprenticeshipSetupModelExtensions
{
    public static CreateUnapprovedApprenticeshipLearningRequest ToApiRequest(this UnapprovedApprenticeshipSetupModel model, Guid? learningKey = null, Guid? episodeKey = null)
    {
        var age = model.Age ?? 18;
        var startDate = model.StartDate ?? new DateTime(2025, 08, 01);
        var endDate = model.EndDate ?? new DateTime(2027, 07, 31);
        var totalPrice = model.TotalPrice ?? 12000;

        return new CreateUnapprovedApprenticeshipLearningRequest
        {
            LearningKey = learningKey ?? Guid.NewGuid(),
            EpisodeKey = episodeKey ?? Guid.NewGuid(),
            ApprovalsApprenticeshipId = 0,
            Learner = new DraftApprenticeshipLearner
            {
                DateOfBirth = startDate.AddYears(age * -1),
                Uln = "1234567890",
                Care = new DraftCare
                {
                    HasEHCP = false,
                    IsCareLeaver = false,
                    CareLeaverEmployerConsentGiven = false
                }
            },
            OnProgramme = new DraftApprenticeshipOnProgramme
            {
                TrainingCode = "123",
                Ukprn = 12345678,
                EmployerAccountId = 0,
                FundingEmployerAccountId = null,
                LegalEntityName = string.Empty,
                EmployerType = EmployerType.Levy,
                FundingBandMaximum = 27000
            },
            Prices =
            [
                new LearningEpisodePrice
                {
                    Key = Guid.NewGuid(),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalPrice = totalPrice,
                    TrainingPrice = totalPrice,
                    EndPointAssessmentPrice = 0
                }
            ],
            PeriodsInLearning =
            [
                new ApprenticeshipPeriodInLearningItem
                {
                    StartDate = startDate,
                    EndDate = null,
                    OriginalExpectedEndDate = endDate
                }
            ]
        };
    }
}
