using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;

public class ApprenticeshipInstalment : BaseInstalment<ApprenticeshipInstalmentEntity>, IDomainEntity<ApprenticeshipInstalmentEntity>
{
    public Guid EpisodePriceKey => _entity.EpisodePriceKey;

    public ApprenticeshipInstalment(short academicYear, byte deliveryPeriod, decimal amount, Guid priceKey, InstalmentType instalmentType = InstalmentType.Regular) : base(academicYear, deliveryPeriod, amount, instalmentType)
    {
        _entity.EpisodePriceKey = priceKey;
    }

    private ApprenticeshipInstalment(ApprenticeshipInstalmentEntity model) : base(model)
    {
    }

    public static ApprenticeshipInstalment Get(ApprenticeshipInstalmentEntity model)
    {
        return new ApprenticeshipInstalment(model);
    }

    public new bool AreSame(ApprenticeshipInstalmentEntity? compare)
    {
        if (compare == null)
            return false;

        return AcademicYear == compare.AcademicYear &&
               DeliveryPeriod == compare.DeliveryPeriod &&
               Amount == compare.Amount &&
               EpisodePriceKey == compare.EpisodePriceKey;
    }
}
