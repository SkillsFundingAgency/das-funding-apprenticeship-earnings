using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Price
{
    private EpisodePriceModel _model;

    public Price(EpisodePriceModel model)
    {
        _model = model;
    }

    public Price(Guid priceKey, DateTime startDate, DateTime endDate, decimal agreedPrice, decimal fundingBandMaximum)
    {
        _model = new EpisodePriceModel
        {
            Key = priceKey,
            StartDate = startDate,
            EndDate = endDate,
            AgreedPrice = agreedPrice,
            FundingBandMaximum = fundingBandMaximum
        };
    }

    public Guid PriceKey => _model.Key;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public decimal AgreedPrice => _model.AgreedPrice;
    public decimal FundingBandMaximum => _model.FundingBandMaximum;

    public static Price Get(EpisodePriceModel model)
    {
        return new Price(model);
    }

    public void Update(DateTime startDate, DateTime endDate, decimal totalPrice, int fundingBandMaximum)
    {
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.AgreedPrice = totalPrice;
        _model.FundingBandMaximum = fundingBandMaximum;
    }

    public EpisodePriceModel GetModel()
    {
        return _model;
    }
}