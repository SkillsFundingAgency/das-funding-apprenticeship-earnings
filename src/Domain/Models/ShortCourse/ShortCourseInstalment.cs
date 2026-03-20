using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

public enum ShortCourseInstalmentType
{
    ThirtyPercentLearningComplete = 0,
    LearningComplete = 1
}

public class ShortCourseInstalment : BaseInstalment<ShortCourseInstalmentEntity>, IDomainEntity<ShortCourseInstalmentEntity>
{
    public new ShortCourseInstalmentType Type => Enum.Parse<ShortCourseInstalmentType>(_entity.Type);
    public bool IsPayable => _entity.IsPayable;

    public ShortCourseInstalment(short academicYear, byte deliveryPeriod, decimal amount, ShortCourseInstalmentType instalmentType = ShortCourseInstalmentType.ThirtyPercentLearningComplete) : base(academicYear, deliveryPeriod, amount, instalmentType.ToString())
    {
    }

    private ShortCourseInstalment(ShortCourseInstalmentEntity model) : base(model)
    {
    }

    public static ShortCourseInstalment Get(ShortCourseInstalmentEntity model)
    {
        return new ShortCourseInstalment(model);
    }

    public void SetIsPayable(bool isPayable) => _entity.IsPayable = isPayable;

    public new bool AreSame(ShortCourseInstalmentEntity? compare)
    {
        if (!base.AreSame(compare))
            return false;

        return IsPayable == compare!.IsPayable;
    }
}
