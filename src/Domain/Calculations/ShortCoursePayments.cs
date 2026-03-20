using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Calculations;

public static class ShortCoursePayments
{
    private const double FirstPaymentDurationPercentage = 0.3;
    private const decimal FirstPaymentPortionPercentage = 0.3m;
    private const decimal SecondPaymentPortionPercentage = 0.7m;

    public static List<ShortCourseInstalment> GenerateShortCoursePayments(decimal totalPrice, DateTime startDate, DateTime endDate, DateTime? completionDate)
    {
        var payments = new List<ShortCourseInstalment>();

        var duration = (endDate - startDate).Days + 1;
        var firstPaymentDate = startDate.AddDays(Math.Floor(duration * FirstPaymentDurationPercentage) - 1);

        payments.Add(new ShortCourseInstalment
        (
            firstPaymentDate.ToAcademicYear(),
            firstPaymentDate.ToDeliveryPeriod(),
            CalculateThirtyPercentInstalmentAmount(totalPrice),
            ShortCourseInstalmentType.ThirtyPercentLearningComplete
        ));

        payments.Add(new ShortCourseInstalment
        (
            completionDate?.ToAcademicYear() ?? endDate.ToAcademicYear(),
            completionDate?.ToDeliveryPeriod() ?? endDate.ToDeliveryPeriod(),
            CalculateCompletionInstalmentAmount(totalPrice),
            ShortCourseInstalmentType.LearningComplete
        ));

        return payments;
    }

    public static void RemoveWithdrawnPayments(List<ShortCourseInstalment> payments, MilestoneFlags milestoneFlags)
    {
        if(!milestoneFlags.HasFlag(MilestoneFlags.ThirtyPercentLearningComplete))
        {
            payments.RemoveAll(p => p.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete);
        }

        if(!milestoneFlags.HasFlag(MilestoneFlags.LearningComplete))
        {
            payments.RemoveAll(p => p.Type == ShortCourseInstalmentType.LearningComplete);
        }
    }

    public static void SetPayability(List<ShortCourseInstalment> payments, bool isApproved, MilestoneFlags milestones)
    {
        foreach (var payment in payments)
        {
            var relevantMilestone = payment.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete
                ? MilestoneFlags.ThirtyPercentLearningComplete
                : MilestoneFlags.LearningComplete;

            payment.SetIsPayable(isApproved && milestones.HasFlag(relevantMilestone));
        }
    }

    public static decimal CalculateThirtyPercentInstalmentAmount(decimal total)
    {
        return total * FirstPaymentPortionPercentage;
    }

    public static decimal CalculateCompletionInstalmentAmount(decimal total)
    {
        return total * SecondPaymentPortionPercentage;
    }
}