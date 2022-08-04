using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class EarningsGeneratedEventBuilder_BuildTests
{
    private EarningsGeneratedEventBuilder _sut;
    private EarningsGeneratedEvent _result;
    private Fixture _fixture;
    private Apprenticeship.Apprenticeship _apprenticeship;

    [SetUp]
    public void SetUp()
    {
        _sut = new EarningsGeneratedEventBuilder();
        _fixture = new Fixture();

        _apprenticeship = _fixture.Create<Apprenticeship.Apprenticeship>();

        _result = _sut.Build(_apprenticeship);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipKeyCorrectly()
    {
        _result.ApprenticeshipKey.Should().Be(_apprenticeship.ApprenticeshipKey);
    }

    [Test]
    public void ShouldPopulateTheUlnCorrectly()
    {
        _result.FundingPeriods.First().Uln.Should().Be(_apprenticeship.Uln);
    }

    [Test]
    public void ShouldPopulateTheEmployerIdCorrectly()
    {
        _result.FundingPeriods.First().EmployerId.Should().Be(_apprenticeship.EmployerAccountId);
    }

    [Test]
    public void ShouldPopulateTheProviderIdCorrectly()
    {
        _result.FundingPeriods.First().ProviderId.Should().Be(_apprenticeship.UKPRN);
    }

    [Test]
    public void ShouldPopulateTheTransferSenderEmployerIdCorrectly()
    {
        _result.FundingPeriods.First().TransferSenderEmployerId.Should().Be(_apprenticeship.FundingEmployerAccountId);
    }

    [Test]
    public void ShouldPopulateTheAgreedPriceCorrectly()
    {
        _result.FundingPeriods.First().AgreedPrice.Should().Be(_apprenticeship.AgreedPrice);
    }

    [Test]
    public void ShouldPopulateTheStartDateCorrectly()
    {
        _result.FundingPeriods.First().StartDate.Should().Be(_apprenticeship.ActualStartDate);
    }

    [Test]
    public void ShouldPopulateTheTrainingCodeCorrectly()
    {
        _result.FundingPeriods.First().TrainingCode.Should().Be(_apprenticeship.TrainingCode);
    }

    [Test]
    public void ShouldPopulateTheEmployerTypeCorrectly()
    {
        _result.FundingPeriods.First().EmployerType.Should().Be(EmployerType.NonLevy);
    }

    [Test]
    public void ShouldBuildTheRightNumberOfDeliveryPeriods()
    {
        _result.FundingPeriods.First().DeliveryPeriods.Count.Should().Be(2);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.FirstOrDefault(x => x.Period == 5).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.FirstOrDefault(x => x.Period == 6).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCalendarMonthCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).CalendarMonth.Should().Be(12);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCalendarMonthCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).CalendarMonth.Should().Be(1);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodCalendarYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).CalenderYear.Should().Be(2019);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodCalendarYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).CalenderYear.Should().Be(2020);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodAcademicYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).AcademicYear.Should().Be(1920);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodAcademicYearCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).AcademicYear.Should().Be(1920);
    }

    [Test]
    public void ShouldPopulateTheFirstDeliveryPeriodLearningAmountCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 5).LearningAmount.Should().Be(1000);
    }

    [Test]
    public void ShouldPopulateTheSecondDeliveryPeriodLearningAmountCorrectly()
    {
        _result.FundingPeriods.First().DeliveryPeriods.First(x => x.Period == 6).LearningAmount.Should().Be(2000);
    }
}