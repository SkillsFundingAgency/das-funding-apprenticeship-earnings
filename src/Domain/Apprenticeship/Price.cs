using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;

public class Price
{
    private ApprenticeshipEpisodePriceEntity _model;

    public Price(ApprenticeshipEpisodePriceEntity model)
    {
        _model = model;
    }

    public Price(Guid priceKey, DateTime startDate, DateTime endDate, decimal agreedPrice)
    {
        _model = new ApprenticeshipEpisodePriceEntity
        {
            Key = priceKey,
            StartDate = startDate,
            EndDate = endDate,
            AgreedPrice = agreedPrice
        };
    }

    public Guid PriceKey => _model.Key;
    public DateTime StartDate => _model.StartDate;
    public DateTime EndDate => _model.EndDate;
    public decimal AgreedPrice => _model.AgreedPrice;

    public static Price Get(ApprenticeshipEpisodePriceEntity model)
    {
        return new Price(model);
    }

    public void Update(DateTime startDate, DateTime endDate, decimal totalPrice)
    {
        _model.StartDate = startDate;
        _model.EndDate = endDate;
        _model.AgreedPrice = totalPrice;
    }

    public ApprenticeshipEpisodePriceEntity GetModel()
    {
        return _model;
    }
}