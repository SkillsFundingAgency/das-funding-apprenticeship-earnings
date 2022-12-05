namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services
{
    public class DateService : IDateService
    {
        public DateTime Today => DateTime.Now.Date;
    }
}
