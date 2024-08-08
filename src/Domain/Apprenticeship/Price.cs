using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Price
    {
        public Price(PriceModel model)
        {
            PriceKey = model.PriceKey;
            StartDate = model.ActualStartDate;
            EndDate = model.PlannedEndDate;
            AgreedPrice = model.AgreedPrice;
            FundingBandMaximum = model.FundingBandMaximum;
        }

        public Price(Guid priceKey, DateTime startDate, DateTime endDate, decimal agreedPrice, decimal fundingBandMaximum)
        {
            PriceKey = priceKey;
            StartDate = startDate;
            EndDate = endDate;
            AgreedPrice = agreedPrice;
            FundingBandMaximum = fundingBandMaximum;
        }

        public void UpdateDates(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        public void UpdatePrice(decimal newAgreedPrice)
        {
            AgreedPrice = newAgreedPrice;
        }

        public void CloseOff(DateTime endDate)
        {
            EndDate = endDate;
        }

        public Guid PriceKey { get; set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal AgreedPrice { get; private set; }
        public decimal FundingBandMaximum { get; }
    }
}
