namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Model;

public class UpdateEnglishAndMathsResponseModel
{
    public bool HasNewEarningsProfileVersionBeenGenerated { get; set; }
    public List<Guid> CreatedCourseKeys { get; set; } = [];
    public List<Guid> ChangedCourseKeys { get; set; } = [];
    public List<Guid> RemovedCourseKeys { get; set; } = [];
    public List<Guid> ReinstatedCourseKeys { get; set; } = [];

    public List<Guid> GetUpdatedCourseKeys() =>
        CreatedCourseKeys
            .Concat(ChangedCourseKeys)
            .Concat(RemovedCourseKeys)
            .Concat(ReinstatedCourseKeys)
            .Distinct()
            .ToList();
}
