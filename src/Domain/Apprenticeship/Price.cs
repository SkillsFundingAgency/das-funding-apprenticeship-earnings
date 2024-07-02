using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship
{
    public class Price
    {
        public Price(PriceModel model)
        {
            ActualStartDate = model.ActualStartDate;
            PlannedEndDate = model.PlannedEndDate;
            AgreedPrice = model.AgreedPrice;
            FundingBandMaximum = model.FundingBandMaximum;
        }

        public DateTime ActualStartDate { get; private set; }
        public DateTime PlannedEndDate { get; private set; }
        public decimal AgreedPrice { get; private set; }
        public decimal FundingBandMaximum { get; }
    }
}
