using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface ICalculateGrowthAndSkillsPaymentsEventBuilder
{
    CalculateGrowthAndSkillsPayments Build(ApprenticeshipLearning learning);
}

public class CalculateGrowthAndSkillsPaymentsEventBuilder : ICalculateGrowthAndSkillsPaymentsEventBuilder
{
    private readonly ISystemClockService _systemClock;

    public CalculateGrowthAndSkillsPaymentsEventBuilder(
        ISystemClockService systemClock)
    {
        _systemClock = systemClock;
    }

    public CalculateGrowthAndSkillsPayments Build(ApprenticeshipLearning learning)
    {
        var episode = learning.GetCurrentEpisode(_systemClock);
        var earnings = BuildEarnings(learning, episode);

        return new CalculateGrowthAndSkillsPayments
        {
            EarningsId = episode.EarningsProfile!.EarningsProfileId,
            UKPRN = episode.UKPRN,
            Learner = new Learner
            {
                LearnerKey = learning.LearningKey,
                ULN = long.Parse(learning.Uln),
                Reference = learning.LearningKey.ToString()
            },
            Training = new Training
            {
                LearningKey = learning.LearningKey,
                CourseType = CourseType.Apprenticeship,
                LearningType = LearningType.Apprenticeship,
                CourseCode = episode.TrainingCode,
                CourseReference = episode.TrainingCode,
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = episode.Prices.First().StartDate,
                PlannedEndDate = episode.Prices.First().EndDate,
                ActualEndDate = episode.LastDayOfLearning,
                TrainingStatus = GetTrainingStatus(episode.IsRemoved, episode.LastDayOfLearning),
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

    private IEnumerable<Earnings> BuildEarnings(ApprenticeshipLearning learning, ApprenticeshipEpisode episode)
    {
        var employerType = episode.FundingType == SFA.DAS.Learning.Types.FundingType.Levy
            ? SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType.Levy
            : SFA.DAS.Payments.EarningEvents.Messages.External.EmployerType.NonLevy;

        var profile = episode.EarningsProfile;

        var earnings = profile!.Instalments
            .GroupBy(i => i.AcademicYear)
            .Select(g => new Earnings
            {
                AcademicYear = g.Key,
                PricePeriods = new List<PricePeriod> {
                    new PricePeriod
                    {
                        Price = episode.Prices.First().AgreedPrice,
                        StartDate = episode.Prices.First().StartDate,
                        EndDate = episode.Prices.First().EndDate,
                        Periods = g.Select(instalment => new EarningPeriod
                        {
                            EarningType = GetEarningType(instalment.Type),
                            DeliveryPeriod = (byte)instalment.DeliveryPeriod,
                            LearningId = learning.ApprovalsApprenticeshipId,
                            Amount = instalment.Amount,
                            Employer = new Employer
                            {
                                AccountId = episode.EmployerAccountId,
                                FundingAccountId = episode.FundingEmployerAccountId ?? episode.EmployerAccountId,
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
            var academicYearDates = GetAcademicYearDates(earningYear.AcademicYear);

            var pricePeriod = earningYear.PricePeriods.Single();

            if (i > 0)
            {
                pricePeriod.StartDate = academicYearDates.StartDate;
            }

            if (i < totalEarnings - 1)
            {
                pricePeriod.EndDate = academicYearDates.EndDate;
            }
        }
    }

    private (DateTime StartDate, DateTime EndDate) GetAcademicYearDates(short year)
    {
        // Parse the first two digits as the start year
        var startYear = 2000 + int.Parse(year.ToString()[..2]);
        var endYear = startYear + 1;

        return (new DateTime(startYear, 8, 1), new DateTime(endYear, 7, 31));
    }

    private static EarningType GetEarningType(InstalmentType instalmentType)
    {
        return instalmentType switch
        {
            InstalmentType.Regular => EarningType.Learning,
            InstalmentType.Completion => EarningType.Completion,
            InstalmentType.Balancing => EarningType.Balancing,
            _ => throw new ArgumentException($"Unknown instalment type: {instalmentType}")
        };
    }
}
