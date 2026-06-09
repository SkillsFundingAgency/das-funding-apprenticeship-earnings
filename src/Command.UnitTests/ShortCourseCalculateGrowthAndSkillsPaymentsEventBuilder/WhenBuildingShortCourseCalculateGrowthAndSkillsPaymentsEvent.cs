using AutoFixture;
using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Payments.EarningEvents.Messages.External;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder;

[TestFixture]
public class WhenBuildingShortCourseCalculateGrowthAndSkillsPaymentsEvent
{
    private Fixture _fixture = null!;
    private Command.ShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _sut = new Command.ShortCourseCalculateGrowthAndSkillsPaymentsEventBuilder();
    }

    [Test]
    public void Then_BasicInformationCorrectlySet()
    {
        // Arrange
        var startDate = new DateTime(2023, 9, 1);
        var endDate = new DateTime(2024, 6, 30);
        var dateOfBirth = new DateTime(2005, 1, 1);

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.TrainingCode, "101")
            .With(x => x.StartDate, startDate)
            .With(x => x.EndDate, endDate)
            .With(x => x.CoursePrice, 1000m)
            .With(x => x.IsRemoved, false)
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.CompletionDate, new DateTime(2024, 6, 28))
            .With(x => x.EarningsProfile, _fixture.Build<ShortCourseEarningsProfileEntity>()
                .With(p => p.Instalments, new List<ShortCourseInstalmentEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.Uln, "1234567890")
            .With(x => x.DateOfBirth, dateOfBirth)
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ShortCourseLearning.Get(learningEntity);
        var episode = learning.GetEpisode();

        // Act
        var result = _sut.Build(episode, learning);

        // Assert
        result.EarningsId.Should().Be(episode.EarningsProfile!.EarningsProfileId);
        result.UKPRN.Should().Be(episode.UKPRN);
        result.EmployerContribution.Should().Be(0);

        result.Learner.Should().NotBeNull();
        result.Learner.LearnerKey.Should().Be(learning.LearningKey);
        result.Learner.ULN.Should().Be(1234567890);
        result.Learner.Reference.Should().Be(learning.LearningKey.ToString());

        result.Training.Should().NotBeNull();
        result.Training.LearningKey.Should().Be(learning.LearningKey);
        result.Training.CourseType.Should().Be(CourseType.ShortCourse);
        result.Training.LearningType.Should().Be(LearningType.ApprenticeshipUnit);
        result.Training.CourseCode.Should().Be(episode.TrainingCode);
        result.Training.CourseReference.Should().Be($"ZSC{int.Parse(episode.TrainingCode.Trim()):D5}");
        result.Training.AgeAtStartOfTraining.Should().Be((byte)episode.AgeAtStartOfApprenticeship);
        result.Training.StartDate.Should().Be(episode.StartDate);
        result.Training.PlannedEndDate.Should().Be(episode.EndDate);
        result.Training.ActualEndDate.Should().Be(episode.CompletionDate);
        result.Training.TrainingStatus.Should().Be(TrainingStatus.Completed);
    }

    [Test]
    public void WhenApprenticeshipIsRemoved_ThenTrainingStatusIsWithdrawn()
    {
        // Arrange
        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.TrainingCode, "101")
            .With(x => x.IsRemoved, true)
            .With(x => x.WithdrawalDate, new DateTime(2024, 3, 1))
            .With(x => x.CompletionDate, (DateTime?)null)
            .With(x => x.EarningsProfile, _fixture.Build<ShortCourseEarningsProfileEntity>()
                .With(p => p.Instalments, new List<ShortCourseInstalmentEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.Uln, "9876543210")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ShortCourseLearning.Get(learningEntity);
        var episode = learning.GetEpisode();

        // Act
        var result = _sut.Build(episode, learning);

        // Assert
        result.Training.TrainingStatus.Should().Be(TrainingStatus.Withdrawn);
        result.Training.ActualEndDate.Should().Be(new DateTime(2024, 3, 1));
    }

    [Test]
    public void WhenApprenticeshipIsActive_ThenTrainingStatusIsContinuing()
    {
        // Arrange
        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.TrainingCode, "101")
            .With(x => x.IsRemoved, false)
            .With(x => x.WithdrawalDate, (DateTime?)null)
            .With(x => x.CompletionDate, (DateTime?)null)
            .With(x => x.EarningsProfile, _fixture.Build<ShortCourseEarningsProfileEntity>()
                .With(p => p.Instalments, new List<ShortCourseInstalmentEntity>())
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.Uln, "1212121212")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ShortCourseLearning.Get(learningEntity);
        var episode = learning.GetEpisode();

        // Act
        var result = _sut.Build(episode, learning);

        // Assert
        result.Training.TrainingStatus.Should().Be(TrainingStatus.Continuing);
        result.Training.ActualEndDate.Should().BeNull();
    }

    [Test]
    public void ThenMapsEarningsCorrectly()
    {
        // Arrange
        var instalment1 = _fixture.Build<ShortCourseInstalmentEntity>()
            .With(x => x.AcademicYear, (short)2324)
            .With(x => x.DeliveryPeriod, (byte)2)
            .With(x => x.Type, ShortCourseInstalmentType.ThirtyPercentLearningComplete.ToString())
            .With(x => x.Amount, 300m)
            .Create();

        var instalment2 = _fixture.Build<ShortCourseInstalmentEntity>()
            .With(x => x.AcademicYear, (short)2324)
            .With(x => x.DeliveryPeriod, (byte)10)
            .With(x => x.Type, ShortCourseInstalmentType.LearningComplete.ToString())
            .With(x => x.Amount, 700m)
            .Create();

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.TrainingCode, "101")
            .With(x => x.StartDate, new DateTime(2023, 9, 1))
            .With(x => x.EndDate, new DateTime(2024, 6, 30))
            .With(x => x.CoursePrice, 1000m)
            .With(x => x.EmployerAccountId, 12345L)
            .With(x => x.FundingEmployerAccountId, 67890L)
            .With(x => x.FundingType, SFA.DAS.Learning.Types.FundingType.Levy)
            .With(x => x.EarningsProfile, _fixture.Build<ShortCourseEarningsProfileEntity>()
                .With(p => p.Instalments, new List<ShortCourseInstalmentEntity> { instalment1, instalment2 })
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.Uln, "3434343434")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ShortCourseLearning.Get(learningEntity);
        var episode = learning.GetEpisode();

        // Act
        var result = _sut.Build(episode, learning);

        // Assert
        result.Earnings.Should().HaveCount(1);
        var earning = result.Earnings.First();
        earning.AcademicYear.Should().Be(2324);
        earning.PricePeriods.Should().HaveCount(1);

        var pricePeriod = earning.PricePeriods.First();
        pricePeriod.Price.Should().Be(1000m);
        pricePeriod.StartDate.Should().Be(episode.StartDate);
        pricePeriod.EndDate.Should().Be(episode.EndDate);
        pricePeriod.Periods.Should().HaveCount(2);

        var period1 = pricePeriod.Periods.First(p => p.DeliveryPeriod == 2);
        period1.EarningType.Should().Be(EarningType.Learning);
        period1.Amount.Should().Be(300m);
        period1.LearningId.Should().Be(learning.ApprovalsApprenticeshipId);
        period1.Employer.AccountId.Should().Be(12345L);
        period1.Employer.FundingAccountId.Should().Be(67890L);
        period1.Employer.EmployerType.Should().Be(EmployerType.Levy);

        var period2 = pricePeriod.Periods.First(p => p.DeliveryPeriod == 10);
        period2.EarningType.Should().Be(EarningType.Completion);
        period2.Amount.Should().Be(700m);
        period2.LearningId.Should().Be(learning.ApprovalsApprenticeshipId);
    }

    [Test]
    public void WhenMultipleAcademicYearsExist_ThenAdjustsStartAndEndDatesAcrossYears()
    {
        // Arrange
        var instalment1 = _fixture.Build<ShortCourseInstalmentEntity>()
            .With(x => x.AcademicYear, (short)2324)
            .With(x => x.DeliveryPeriod, (byte)11)
            .With(x => x.Type, ShortCourseInstalmentType.ThirtyPercentLearningComplete.ToString())
            .With(x => x.Amount, 300m)
            .Create();

        var instalment2 = _fixture.Build<ShortCourseInstalmentEntity>()
            .With(x => x.AcademicYear, (short)2425)
            .With(x => x.DeliveryPeriod, (byte)2)
            .With(x => x.Type, ShortCourseInstalmentType.LearningComplete.ToString())
            .With(x => x.Amount, 700m)
            .Create();

        var episodeEntity = _fixture.Build<ShortCourseEpisodeEntity>()
            .With(x => x.TrainingCode, "101")
            .With(x => x.StartDate, new DateTime(2023, 9, 1))
            .With(x => x.EndDate, new DateTime(2024, 10, 31))
            .With(x => x.CoursePrice, 1000m)
            .With(x => x.EarningsProfile, _fixture.Build<ShortCourseEarningsProfileEntity>()
                .With(p => p.Instalments, new List<ShortCourseInstalmentEntity> { instalment1, instalment2 })
                .Create())
            .Create();

        var learningEntity = _fixture.Build<ShortCourseLearningEntity>()
            .With(x => x.Uln, "5656565656")
            .With(x => x.Episodes, new List<ShortCourseEpisodeEntity> { episodeEntity })
            .Create();

        var learning = ShortCourseLearning.Get(learningEntity);
        var episode = learning.GetEpisode();

        // Act
        var result = _sut.Build(episode, learning);

        // Assert
        result.Earnings.Should().HaveCount(2);

        var firstYearEarning = result.Earnings.First(e => e.AcademicYear == 2324);
        var firstPricePeriod = firstYearEarning.PricePeriods.Single();
        firstPricePeriod.StartDate.Should().Be(episode.StartDate);
        // Adjusted to last day of academic year 23/24 (since there's a subsequent year)
        firstPricePeriod.EndDate.Should().Be(new DateTime(2024, 7, 31));

        var secondYearEarning = result.Earnings.First(e => e.AcademicYear == 2425);
        var secondPricePeriod = secondYearEarning.PricePeriods.Single();
        // Adjusted to first day of academic year 24/25 (since there's a prior year)
        secondPricePeriod.StartDate.Should().Be(new DateTime(2024, 8, 1));
        secondPricePeriod.EndDate.Should().Be(episode.EndDate);
    }
}
