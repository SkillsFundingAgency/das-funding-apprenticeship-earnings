using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using static SFA.DAS.Funding.ApprenticeshipEarnings.Types.EmployerTypeExtensions;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder
{
    CalculateGrowthAndSkillsPayments Build(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference);
    CalculateGrowthAndSkillsPayments BuildForEnglishAndMaths(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, EnglishAndMathsDomainModel course, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference);
}

public class ApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder : IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder
{
    private const string ApprenticeshipCourseReference = "ZPROG001";

    public CalculateGrowthAndSkillsPayments Build(ApprenticeshipEpisode episode, ApprenticeshipLearning learning,
        long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference)
    {
        return new CalculateGrowthAndSkillsPayments
        {
            EarningsId = episode.EarningsProfile!.Version,
            UKPRN = episode.UKPRN,
            Learner = CreateLearnerEventDetails(learning, learnerKey, learnerReference),
            Training = new Training
            {
                LearningKey = learning.LearningKey,
                CourseType = CourseType.Apprenticeship,
                LearningType = LearningType.Apprenticeship,
                CourseCode = episode.TrainingCode.Trim(),
                CourseReference = ApprenticeshipCourseReference,
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = episode.Prices.Min(p => p.StartDate),
                PlannedEndDate = GetPlannedEndDate(episode),
                ActualEndDate = episode.WithdrawalDate ?? episode.CompletionDate,
                TrainingStatus = GetTrainingStatus(episode.WithdrawalDate, episode.CompletionDate, episode.PauseDate)
            },
            EmployerContribution = 0,
            Earnings = BuildEarnings(episode, learning, employerAccountId, fundingAccountId)
        };
    }

    public CalculateGrowthAndSkillsPayments BuildForEnglishAndMaths(ApprenticeshipEpisode episode, ApprenticeshipLearning learning,
        EnglishAndMathsDomainModel course, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference)
    {
        return new CalculateGrowthAndSkillsPayments
        {
            EarningsId = course.Version,
            UKPRN = episode.UKPRN,
            Learner = CreateLearnerEventDetails(learning, learnerKey, learnerReference),
            Training = new Training
            {
                LearningKey = course.Key,
                CourseType = CourseType.FunctionalSkill,
                LearningType = LearningType.MathsAndEnglish,
                CourseCode = course.LearnAimRef.Trim(),
                CourseReference = course.LearnAimRef.Trim(),
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = course.StartDate,
                PlannedEndDate = course.EndDate,
                ActualEndDate = course.WithdrawalDate ?? course.CompletionDate,
                TrainingStatus = GetTrainingStatus(course.WithdrawalDate, course.CompletionDate, episode.PauseDate)
            },
            EmployerContribution = 0,
            Earnings = BuildEnglishAndMathsEarnings(episode, course, learning, employerAccountId, fundingAccountId)
        };
    }

    /// <summary>
    /// Periods in learning are cleared when a learner is removed from the ILR, in which case the latest price end date is used instead.
    /// </summary>
    private static DateTime GetPlannedEndDate(ApprenticeshipEpisode episode)
    {
        var latestPeriodInLearning = episode.EpisodePeriodsInLearning.MaxBy(o => o.StartDate);

        return latestPeriodInLearning?.OriginalExpectedEndDate ?? episode.Prices.Max(p => p.EndDate);
    }

    private static TrainingStatus GetTrainingStatus(DateTime? withdrawalDate, DateTime? completionDate, DateTime? pauseDate)
    {
        if (withdrawalDate is not null)
        {
            return TrainingStatus.Withdrawn;
        }

        if (completionDate is not null)
        {
            return TrainingStatus.Completed;
        }

        return pauseDate is null ? TrainingStatus.Continuing : TrainingStatus.BreakInLearning;
    }

    private static IList<Earnings> BuildEarnings(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, long employerAccountId, long fundingAccountId)
    {
        var prices = episode.Prices.ToDictionary(p => p.PriceKey);
        var periodsInLearning = episode.EpisodePeriodsInLearning.OrderBy(p => p.StartDate).ToList();

        var onProgrammeEntries = episode.EarningsProfile!.Instalments
            .Select(instalment => (
                instalment.AcademicYear,
                instalment.EpisodePriceKey,
                EarningType: GetEarningType(instalment.Type),
                instalment.DeliveryPeriod,
                instalment.Amount));

        var incentiveEntries = GetIncentiveEntries(episode);
        var learningSupportEntries = GetLearningSupportEntries(episode);

        return onProgrammeEntries.Concat(incentiveEntries).Concat(learningSupportEntries)
            .Where(x => x.Amount != 0) // £0 earnings are reported on FM36 but not sent to payments
            .GroupBy(x => x.AcademicYear)
            .Select(yearGroup => new Earnings
            {
                AcademicYear = yearGroup.Key,
                // Each period in learning is sent as its own price period, so a return from a break in learning
                // is reported with the instalments recalculated for that return
                PricePeriods = yearGroup
                    .GroupBy(x => (x.EpisodePriceKey, PeriodInLearning: GetPeriodInLearning(periodsInLearning, x.AcademicYear, x.DeliveryPeriod)))
                    .Select(priceGroup =>
                    {
                        var price = prices[priceGroup.Key.EpisodePriceKey];
                        var periodInLearning = priceGroup.Key.PeriodInLearning;
                        var regularInstalmentsForPrice = episode.EarningsProfile.Instalments
                            .Where(x => x.EpisodePriceKey == price.PriceKey && x.Type == InstalmentType.Regular
                                && GetPeriodInLearning(periodsInLearning, x.AcademicYear, x.DeliveryPeriod) == periodInLearning)
                            .ToList();

                        return new PricePeriod
                        {
                            Price = price.AgreedPrice,
                            StartDate = GetPricePeriodStartDate(price, periodInLearning),
                            EndDate = GetPricePeriodEndDate(price, periodInLearning, periodsInLearning),
                            CompletionAmount = new ApprenticeshipFunding(price.AgreedPrice, episode.FundingBandMaximum).CompletionPayment,
                            InstalmentAmount = regularInstalmentsForPrice.Count > 0 ? regularInstalmentsForPrice.Max(o => o.Amount) : 0,
                            NumberOfInstalments = regularInstalmentsForPrice.Count,
                            Periods = priceGroup.Select(x => new EarningPeriod
                            {
                                EarningType = x.EarningType,
                                DeliveryPeriod = x.DeliveryPeriod,
                                LearningId = learning.ApprovalsApprenticeshipId,
                                Amount = x.Amount,
                                Employer = new Employer
                                {
                                    AccountId = employerAccountId,
                                    FundingAccountId = fundingAccountId,
                                    EmployerType = episode.EmployerType.ToPaymentsEmployerType()
                                }
                            }).ToList()
                        };
                    })
                    .OrderBy(p => p.StartDate)
                    .ToList()
            })
            .OrderBy(e => e.AcademicYear)
            .ToList();
    }

    /// <summary>
    /// Earnings hold no reference to the period in learning they were earned in, so they are matched on delivery period instead.
    /// Instalments are only generated for whole months within a period in learning, so periods never share a delivery period and
    /// anything due after a period ends (e.g. completion or balancing) belongs to the latest period started by then.
    /// </summary>
    private static ApprenticeshipPeriodInLearning? GetPeriodInLearning(List<ApprenticeshipPeriodInLearning> periodsInLearning, short academicYear, byte deliveryPeriod)
    {
        var deliveryMonth = academicYear.ToDateTime(deliveryPeriod);

        return periodsInLearning.LastOrDefault(p => new DateTime(p.StartDate.Year, p.StartDate.Month, 1) <= deliveryMonth)
            ?? periodsInLearning.FirstOrDefault();
    }

    private static DateTime GetPricePeriodStartDate(ApprenticeshipPrice price, ApprenticeshipPeriodInLearning? periodInLearning)
    {
        return periodInLearning != null && periodInLearning.StartDate > price.StartDate
            ? periodInLearning.StartDate
            : price.StartDate;
    }

    private static DateTime GetPricePeriodEndDate(ApprenticeshipPrice price, ApprenticeshipPeriodInLearning? periodInLearning, List<ApprenticeshipPeriodInLearning> periodsInLearning)
    {
        if (periodInLearning is null || periodInLearning == periodsInLearning.Last())
        {
            return price.EndDate;
        }

        return periodInLearning.EffectiveEndDate < price.EndDate
            ? periodInLearning.EffectiveEndDate
            : price.EndDate;
    }

    private static IEnumerable<(short AcademicYear, Guid EpisodePriceKey, EarningType EarningType, byte DeliveryPeriod, decimal Amount)> GetIncentiveEntries(
        ApprenticeshipEpisode episode)
    {
        foreach (var incentiveType in new[] { InstalmentTypes.ProviderIncentive, InstalmentTypes.EmployerIncentive })
        {
            var payments = episode.EarningsProfile!.AdditionalPayments
                .Where(x => x.AdditionalPaymentType == incentiveType)
                .OrderBy(x => x.DueDate)
                .ToList();

            for (var i = 0; i < payments.Count; i++)
            {
                var payment = payments[i];
                var price = episode.GetPriceAt(payment.DueDate);

                yield return (
                    payment.AcademicYear,
                    price.PriceKey,
                    GetIncentiveEarningType(incentiveType, i),
                    payment.DeliveryPeriod,
                    payment.Amount);
            }
        }
    }

    private static IEnumerable<(short AcademicYear, Guid EpisodePriceKey, EarningType EarningType, byte DeliveryPeriod, decimal Amount)> GetLearningSupportEntries(
        ApprenticeshipEpisode episode)
    {
        return episode.EarningsProfile!.AdditionalPayments
            .Where(x => x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .OrderBy(x => x.DueDate)
            .Select(payment => (
                payment.AcademicYear,
                episode.GetPriceAt(payment.DueDate).PriceKey,
                EarningType: EarningType.LearningSupport,
                payment.DeliveryPeriod,
                payment.Amount));
    }

    private static EarningType GetIncentiveEarningType(string additionalPaymentType, int occurrenceIndex)
    {
        return (additionalPaymentType, occurrenceIndex) switch
        {
            (InstalmentTypes.ProviderIncentive, 0) => EarningType.First16To18ProviderIncentive,
            (InstalmentTypes.ProviderIncentive, 1) => EarningType.Second16To18ProviderIncentive,
            (InstalmentTypes.EmployerIncentive, 0) => EarningType.First16To18EmployerIncentive,
            (InstalmentTypes.EmployerIncentive, 1) => EarningType.Second16To18EmployerIncentive,
            _ => throw new ArgumentException($"Unexpected {additionalPaymentType} incentive occurrence: {occurrenceIndex}")
        };
    }

    private static IList<Earnings> BuildEnglishAndMathsEarnings(ApprenticeshipEpisode episode, EnglishAndMathsDomainModel course, ApprenticeshipLearning learning, long employerAccountId, long fundingAccountId)
    {
        var regularInstalments = course.Instalments
            .Where(x => x.Type == EnglishAndMathsInstalmentType.Regular)
            .ToList();

        var instalmentEntries = course.Instalments
            .Select(instalment => (
                instalment.AcademicYear,
                EarningType: GetEnglishAndMathsEarningType(instalment.Type),
                instalment.DeliveryPeriod,
                instalment.Amount));

        var learningSupportEntries = course.AdditionalPayments
            .Where(x => x.AdditionalPaymentType == InstalmentTypes.LearningSupport)
            .OrderBy(x => x.DueDate)
            .Select(payment => (
                payment.AcademicYear,
                EarningType: EarningType.LearningSupport,
                payment.DeliveryPeriod,
                payment.Amount));

        return instalmentEntries.Concat(learningSupportEntries)
            .Where(x => x.Amount != 0) // £0 earnings are reported on FM36 but not sent to payments
            .GroupBy(x => x.AcademicYear)
            .Select(yearGroup => new Earnings
            {
                AcademicYear = yearGroup.Key,
                PricePeriods = new List<PricePeriod>
                {
                    new PricePeriod
                    {
                        Price = course.Amount,
                        StartDate = course.StartDate,
                        EndDate = course.EndDate,
                        CompletionAmount = 0,
                        InstalmentAmount = regularInstalments.Count > 0 ? regularInstalments.Max(o => o.Amount) : 0,
                        NumberOfInstalments = regularInstalments.Count,
                        Periods = yearGroup.Select(instalment => new EarningPeriod
                        {
                            EarningType = instalment.EarningType,
                            DeliveryPeriod = instalment.DeliveryPeriod,
                            LearningId = learning.ApprovalsApprenticeshipId,
                            Amount = instalment.Amount,
                            Employer = new Employer
                            {
                                AccountId = employerAccountId,
                                FundingAccountId = fundingAccountId,
                                EmployerType = episode.EmployerType.ToPaymentsEmployerType()
                            }
                        }).ToList()
                    }
                }
            })
            .OrderBy(e => e.AcademicYear)
            .ToList();
    }

    private static EarningType GetEarningType(InstalmentType instalmentType) =>
        instalmentType switch
        {
            InstalmentType.Regular => EarningType.Learning,
            InstalmentType.Completion => EarningType.Completion,
            InstalmentType.Balancing => EarningType.Balancing,
            _ => throw new ArgumentException($"Unknown instalment type: {instalmentType}")
        };

    private static EarningType GetEnglishAndMathsEarningType(EnglishAndMathsInstalmentType instalmentType) =>
        instalmentType switch
        {
            EnglishAndMathsInstalmentType.Regular => EarningType.OnProgrammeMathsAndEnglish,
            EnglishAndMathsInstalmentType.Balancing => EarningType.BalancingMathsAndEnglish,
            _ => throw new ArgumentException($"Unknown instalment type: {instalmentType}")
        };

    private static Learner CreateLearnerEventDetails(ApprenticeshipLearning learning, Guid learnerKey, string learnerReference) =>
        new()
        {
            LearnerKey = learnerKey,
            ULN = long.Parse(learning.Uln),
            Reference = learnerReference
        };
}
