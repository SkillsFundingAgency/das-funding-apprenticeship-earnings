using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UnapprovedShortCourseSetupModel
{
    public int? Age { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? CompletionDate { get; set; }
}

public static class UnapprovedShortCourseSetupModelExtensions
{
    public static CreateUnapprovedShortCourseLearningRequest ToApiRequest(this UnapprovedShortCourseSetupModel model)
    {
        var age = model.Age ?? 18;
        var startDate = model.StartDate ?? new DateTime(2022, 01, 01);
        var expectedEndDate = model.ExpectedEndDate ?? new DateTime(2022, 06, 30);
        var totalPrice = model.TotalPrice ?? 1000;

        return new CreateUnapprovedShortCourseLearningRequest
        {
            LearningKey = Guid.NewGuid(),
            Learner = new Learner
            {
                DateOfBirth = startDate.AddYears(age * -1),
                Uln = "1002"
            },
            LearningSupport = new List<LearningSupportItem>(),
            OnProgramme = new OnProgramme
            {
                CourseCode = "UX1",
                EmployerId = 100112,
                ExpectedEndDate = expectedEndDate,
                Milestones = new List<Milestone>(),
                StartDate = startDate,
                TotalPrice = totalPrice,
                Ukprn = 10000114,
                CompletionDate = model.CompletionDate
            }
        };
    }
}