namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services
{
    public interface IAcademicYearService
    {
        public short CurrentAcademicYear { get; }
        public DateTime StartOfCurrentAcademicYear(DateTime currentDate);
        public DateTime EndOfCurrentAcademicYear(DateTime currentDate);
    }
}
