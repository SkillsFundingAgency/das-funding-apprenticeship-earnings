using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;

public class GetShortCourseEarningsResponse
{
    public List<ShortCourseEarning> Earnings { get; set; } = new();
}
