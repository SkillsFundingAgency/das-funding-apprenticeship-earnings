namespace SFA.DAS.Funding.ApprenticeshipEarnings.DataTransferObjects
{
    public class ProviderEarningsSummary
    {
        public decimal TotalEarningsForCurrentAcademicYear { get; set; }
        public decimal TotalLevyEarningsForCurrentAcademicYear { get; set; }
        public decimal TotalNonLevyEarningsForCurrentAcademicYear { get; set; }
        public decimal TotalNonLevyEarningsForCurrentAcademicYearGovernment { get; set; }
        public decimal TotalNonLevyEarningsForCurrentAcademicYearEmployer { get; set; }
    }
}