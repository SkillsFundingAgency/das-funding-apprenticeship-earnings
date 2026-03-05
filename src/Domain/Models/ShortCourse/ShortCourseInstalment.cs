using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public class ShortCourseInstalment : BaseInstalment<ShortCourseInstalmentEntity>, IDomainEntity<ShortCourseInstalmentEntity>
{
    public ShortCourseInstalment(short academicYear, byte deliveryPeriod, decimal amount, InstalmentType instalmentType = InstalmentType.Regular) : base(academicYear, deliveryPeriod, amount, instalmentType)
    {
    }

    private ShortCourseInstalment(ShortCourseInstalmentEntity model) : base(model)
    {
    }

    public static ShortCourseInstalment Get(ShortCourseInstalmentEntity model)
    {
        return new ShortCourseInstalment(model);
    }
}