using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using EnglishAndMathsDomainModel = SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.EnglishAndMaths;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;
using static SFA.DAS.Funding.ApprenticeshipEarnings.Types.EmployerTypeExtensions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command;

public interface IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder
{
    CalculateGrowthAndSkillsPayments Build(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference);
    CalculateGrowthAndSkillsPayments BuildForEnglishAndMaths(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, EnglishAndMathsDomainModel course, long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference);
}

public class ApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder : IApprenticeshipCalculateGrowthAndSkillsPaymentsEventBuilder
{
    private const string ApprenticeshipCourseReference = "ZPROG0001";

    public CalculateGrowthAndSkillsPayments Build(ApprenticeshipEpisode episode, ApprenticeshipLearning learning,
        long employerAccountId, long fundingAccountId, Guid learnerKey, string learnerReference)
    {
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
                CourseType = CourseType.Apprenticeship,
                LearningType = LearningType.Apprenticeship,
                CourseCode = episode.TrainingCode.Trim(),
                CourseReference = ApprenticeshipCourseReference,
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = episode.Prices.Min(p => p.StartDate),
                PlannedEndDate = episode.LastDayOfLearning ?? episode.Prices.Max(p => p.EndDate),
                ActualEndDate = episode.WithdrawalDate ?? episode.CompletionDate,
                TrainingStatus = GetTrainingStatus(episode.WithdrawalDate, episode.CompletionDate)
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
                CourseType = CourseType.Apprenticeship,
                LearningType = LearningType.MathsAndEnglish,
                CourseCode = course.LearnAimRef, //todo this is not LARs code, needs to change
                CourseReference = course.LearnAimRef, // per design doc: for EnglishAndMaths this is the LearnAimRef for the course
                AgeAtStartOfTraining = (byte)episode.AgeAtStartOfApprenticeship,
                StartDate = course.StartDate,
                PlannedEndDate = course.EndDate,
                ActualEndDate = course.ActualEndDate,
                TrainingStatus = GetTrainingStatus(course.WithdrawalDate, course.CompletionDate)
            },
            EmployerContribution = 0,
            Earnings = BuildEnglishAndMathsEarnings(episode, course, learning, employerAccountId, fundingAccountId)
        };
    }

    private static TrainingStatus GetTrainingStatus(DateTime? withdrawalDate, DateTime? completionDate)
    {
        if (withdrawalDate != null)
            return TrainingStatus.Withdrawn;

        if (completionDate != null)
            return TrainingStatus.Completed;

        return TrainingStatus.Continuing;
    }

    private static IList<Earnings> BuildEarnings(ApprenticeshipEpisode episode, ApprenticeshipLearning learning, long employerAccountId, long fundingAccountId)
    {
        var prices = episode.Prices.ToDictionary(p => p.PriceKey);

        var onProgrammeEntries = episode.EarningsProfile!.Instalments
            .Select(instalment => (
                instalment.AcademicYear,
                EpisodePriceKey: instalment.EpisodePriceKey,
                EarningType: GetEarningType(instalment.Type),
                instalment.DeliveryPeriod,
                instalment.Amount));

        // FLP-2030: only the four 16-18 incentive earning types are in scope. The same ProviderIncentive/
        // EmployerIncentive additional payments are also generated for 19-24 EHCP/care leaver apprentices
        // (see IncentivePayments.Generate19To24IncentivePayments), but there is no equivalent PV2 earning
        // type for those in this ticket, so they're deliberately excluded here based on age at start.
        var incentiveEntries = episode.AgeAtStartOfApprenticeship <= 18
            ? GetIncentiveEntries(episode, prices)
            : Enumerable.Empty<(short AcademicYear, Guid EpisodePriceKey, EarningType EarningType, byte DeliveryPeriod, decimal Amount)>();

        return onProgrammeEntries.Concat(incentiveEntries)
            .GroupBy(x => x.AcademicYear)
            .Select(yearGroup => new Earnings
            {
                AcademicYear = yearGroup.Key,
                PricePeriods = yearGroup
                    .GroupBy(x => x.EpisodePriceKey)
                    .Select(priceGroup =>
                    {
                        var price = prices[priceGroup.Key];
                        return new PricePeriod
                        {
                            Price = price.AgreedPrice,
                            StartDate = price.StartDate,
                            EndDate = price.EndDate,
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
                    .ToList()
            })
            .OrderBy(e => e.AcademicYear)
            .ToList();
    }

    private static IEnumerable<(short AcademicYear, Guid EpisodePriceKey, EarningType EarningType, byte DeliveryPeriod, decimal Amount)> GetIncentiveEntries(
        ApprenticeshipEpisode episode, IReadOnlyDictionary<Guid, ApprenticeshipPrice> prices)
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
                var price = GetPriceForDate(prices.Values, payment.DueDate);

                yield return (
                    payment.AcademicYear,
                    price.PriceKey,
                    GetIncentiveEarningType(incentiveType, i),
                    payment.DeliveryPeriod,
                    payment.Amount);
            }
        }
    }

    private static ApprenticeshipPrice GetPriceForDate(IEnumerable<ApprenticeshipPrice> prices, DateTime date)
    {
        return prices.FirstOrDefault(p => p.StartDate <= date && date <= p.EndDate)
            ?? prices.OrderBy(p => p.StartDate).Last(p => p.StartDate <= date);
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
        return course.Instalments
            .GroupBy(i => i.AcademicYear)
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
                        Periods = yearGroup.Select(instalment => new EarningPeriod
                        {
                            EarningType = GetEnglishAndMathsEarningType(instalment.Type),
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

    private static EarningType GetEnglishAndMathsEarningType(EnglishAndMathsInstalmentType instalmentType)
    {
        return instalmentType switch
        {
            EnglishAndMathsInstalmentType.Regular => EarningType.OnProgrammeMathsAndEnglish,
            EnglishAndMathsInstalmentType.Balancing => EarningType.BalancingMathsAndEnglish,
            _ => throw new ArgumentException($"Unknown instalment type: {instalmentType}")
        };
    }
}
