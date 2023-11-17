using SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetAcademicYearEarnings
{
    public class GetAcademicYearEarningsResponse
    {
        public GetAcademicYearEarningsResponse(AcademicYearEarnings academicYearEarnings)
        {
            AcademicYearEarnings = academicYearEarnings;
        }

        public DataTransferObjects.AcademicYearEarnings AcademicYearEarnings { get; }
    }
}
