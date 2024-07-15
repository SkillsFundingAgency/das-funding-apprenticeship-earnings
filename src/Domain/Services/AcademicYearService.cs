using Microsoft.Extensions.Logging;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IDateService _dateService;
        private readonly ILogger<AcademicYearService> _logger;

        public AcademicYearService(IDateService dateService, ILogger<AcademicYearService> logger)
        {
            _dateService = dateService;
            _logger = logger;
        }

        public short CurrentAcademicYear => GetCurrentAcademicYear();

        private short GetCurrentAcademicYear()
        {
            try
            {
                return _dateService.Today.ToAcademicYear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get current academic year");
                throw;
            }
            
        }
    }
}
