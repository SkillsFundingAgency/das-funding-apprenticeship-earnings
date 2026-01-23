using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.ApprenticeshipFunding;

public class PriceInPeriod
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal AgreedPrice { get; private set; }
    public Guid PriceKey { get; private set; }
    public DateTime OriginalExpectedEndDate { get; set; }

    public PriceInPeriod(Price price, DateTime periodStartDate, DateTime periodEndDate, DateTime originalExpectedEndDate)
    {
        StartDate = price.StartDate > periodStartDate ? price.StartDate : periodStartDate;
        EndDate = price.EndDate < periodEndDate ? price.EndDate : periodEndDate;
        AgreedPrice = price.AgreedPrice;
        PriceKey = price.PriceKey;
        OriginalExpectedEndDate = originalExpectedEndDate;
    }
}