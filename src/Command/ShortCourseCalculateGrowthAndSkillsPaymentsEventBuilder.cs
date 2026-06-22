using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder
{
    CalculateGrowthAndSkillsPayments Build(ShortCourseEpisode episode, ShortCourseLearning learning, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference);
}

public class ShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder : IShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder
{
    public CalculateGrowthAndSkillsPayments Build(ShortCourseEpisode episode, ShortCourseLearning learning,
        long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference)
    {
        var earnings = BuildEarnings(learning, episode, employerAccountId, fundingAccountId);

        return new CalculateGrowthAndSkillsPayments
        {
            EarningsId = episode.EarningsProfile!.Version,
            UKPRN = episode.UKPRN,
            Learner = new Learner
            {
                LearnerKey = learnerKey,
                ULN = long.Parse(learning.Uln),
                Reference = learnerReference
            },
            Training = new Training
            {
                LearningKey = learning.LearningKey,
                CourseType = CourseType.ShortCourse,
                LearningType = LearningType.ApprenticeshipUnit,
                CourseCode = learning.TrainingCode.Trim(),
                CourseReference = learning.TrainingCode.Trim(),
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = episode.StartDate,
                PlannedEndDate = episode.EndDate,
                ActualEndDate = episode.WithdrawalDate ?? episode.CompletionDate,
                TrainingStatus = GetTrainingStatus(episode.IsRemoved, episode.WithdrawalDate ?? episode.CompletionDate),
            },
            EmployerContribution = 0,
            Earnings = earnings,
        };
    }

    private TrainingStatus GetTrainingStatus(bool isRemoved, DateTime? lastDayOfLearning)
    {
        if (isRemoved)
            return TrainingStatus.Withdrawn;

        if (lastDayOfLearning != null)
            return TrainingStatus.Completed;

        return TrainingStatus.Continuing;
    }

    private IEnumerable<Earnings> BuildEarnings(ShortCourseLearning learning, ShortCourseEpisode episode, long employerAccountId, long fundingAccountId)
    {
        var employerType = episode.FundingType == Learning.Types.FundingType.Levy
            ? EmployerType.Levy
            : EmployerType.NonLevy;

        var profile = episode.EarningsProfile;

        var earnings = profile!.Instalments
            .Where(i => i.IsPayable)
            .GroupBy(i => i.AcademicYear)
            .Select(g => new Earnings
            {
                AcademicYear = g.Key,
                PricePeriods = new List<PricePeriod> {
                    new PricePeriod
                    {
                        Price = episode.CoursePrice,
                        StartDate = episode.StartDate,
                        EndDate = episode.EndDate,
                        Periods = g.Select(instalment => new EarningPeriod
                        {
                            EarningType = GetEarningType(instalment.Type),
                            DeliveryPeriod = instalment.DeliveryPeriod,
                            LearningId = learning.ApprovalsApprenticeshipId,
                            Amount = instalment.Amount,
                            Employer = new Employer
                            {
                                AccountId = employerAccountId,
                                FundingAccountId = fundingAccountId,
                                EmployerType = employerType
                            }
                        }).ToList()
                    }
                }
            })
            .OrderBy(e => e.AcademicYear)
            .ToList();

        if (earnings.Count > 1)
            SetStartEndDatesForMultipleYears(earnings);

        return earnings;
    }

    private void SetStartEndDatesForMultipleYears(List<Earnings> earnings)
    {
        var totalEarnings = earnings.Count;

        for (var i = 0; i < totalEarnings; i++)
        {
            var earningYear = earnings[i];
            var academicYear = earningYear.AcademicYear.GetAcademicYear();

            var pricePeriod = earningYear.PricePeriods.Single();

            if (i > 0)
            {
                pricePeriod.StartDate = academicYear.StartDate;
            }

            if (i < totalEarnings - 1)
            {
                pricePeriod.EndDate = academicYear.EndDate;
            }
        }
    }

    private static EarningType GetEarningType(ShortCourseInstalmentType instalmentType)
    {
        return instalmentType switch
        {
            ShortCourseInstalmentType.ThirtyPercentLearningComplete => EarningType.Milestone1,
            ShortCourseInstalmentType.LearningComplete => EarningType.Completion,
            _ => throw new ArgumentException($"Unknown instalment type: {instalmentType}")
        };
    }
}
