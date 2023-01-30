using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings
{
    public class GetAcademicYearEarningsRequest : IQuery
    {
        public long Ukprn { get; }

        public GetAcademicYearEarningsRequest(long ukprn)
        {
            Ukprn = ukprn;
        }
    }
}
