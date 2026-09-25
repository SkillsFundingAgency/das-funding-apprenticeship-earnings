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

        return episode.EarningsProfile!.Instalments
            .GroupBy(i => i.AcademicYear)
            .Select(yearGroup => new Earnings
            {
                AcademicYear = yearGroup.Key,
                PricePeriods = yearGroup
                    .GroupBy(i => i.EpisodePriceKey)
                    .Select(priceGroup =>
                    {
                        var price = prices[priceGroup.Key];
                        return new PricePeriod
                        {
                            Price = price.AgreedPrice,
                            StartDate = price.StartDate,
                            EndDate = price.EndDate,
                            Periods = priceGroup.Select(instalment => new EarningPeriod
                            {
                                EarningType = GetEarningType(instalment.Type),
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
                        };
                    })
                    .ToList()
            })
            .OrderBy(e => e.AcademicYear)
            .ToList();
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
