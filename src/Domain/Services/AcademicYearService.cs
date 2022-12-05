namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IDateService _dateService;

        public AcademicYearService(IDateService dateService)
        {
            _dateService = dateService;
        }

        public short CurrentAcademicYear => _dateService.Today.ToAcademicYear();
    }
}
