using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipPrice
{
    private ApprenticeshipEpisodePriceEntity _entity;

    public Guid PriceKey => _entity.Key;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public decimal AgreedPrice => _entity.AgreedPrice;

    public ApprenticeshipPrice(ApprenticeshipEpisodePriceEntity entity)
    {
        _entity = entity;
    }

    public ApprenticeshipPrice(Guid priceKey, DateTime startDate, DateTime endDate, decimal agreedPrice)
    {
        _entity = new ApprenticeshipEpisodePriceEntity
        {
            Key = priceKey,
            StartDate = startDate,
            EndDate = endDate,
            AgreedPrice = agreedPrice
        };
    }
    public static ApprenticeshipPrice Get(ApprenticeshipEpisodePriceEntity model)
    {
        return new ApprenticeshipPrice(model);
    }

    public void Update(DateTime startDate, DateTime endDate, decimal totalPrice)
    {
        _entity.StartDate = startDate;
        _entity.EndDate = endDate;
        _entity.AgreedPrice = totalPrice;
    }

    public ApprenticeshipEpisodePriceEntity GetEntity()
    {
        return _entity;
    }
}