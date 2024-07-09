using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Price
    {
        public Price(PriceModel model)
        {
            PriceKey = model.PriceKey;
            ActualStartDate = model.ActualStartDate;
            PlannedEndDate = model.PlannedEndDate;
            AgreedPrice = model.AgreedPrice;
            FundingBandMaximum = model.FundingBandMaximum;
        }

        public Price(Guid priceKey, DateTime actualStartDate, DateTime plannedEndDate, decimal agreedPrice, decimal fundingBandMaximum)
        {
            PriceKey = priceKey;
            ActualStartDate = actualStartDate;
            PlannedEndDate = plannedEndDate;
            AgreedPrice = agreedPrice;
            FundingBandMaximum = fundingBandMaximum;
        }

        public void UpdateDates(DateTime actualStartDate, DateTime plannedEndDate)
        {
            ActualStartDate = actualStartDate;
            PlannedEndDate = plannedEndDate;
        }

        public void CloseOff(DateTime plannedEndDate)
        {
            PlannedEndDate = plannedEndDate;
        }

        public Guid PriceKey { get; set; }
        public DateTime ActualStartDate { get; private set; }
        public DateTime PlannedEndDate { get; private set; }
        public decimal AgreedPrice { get; private set; }
        public decimal FundingBandMaximum { get; }
    }
}
