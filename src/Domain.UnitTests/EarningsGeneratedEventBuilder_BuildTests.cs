using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Apprenticeships.Types;
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

        _apprenticeship = new Apprenticeship.Apprenticeship(
            Guid.NewGuid(),
            _fixture.Create<long>(),
            _fixture.Create<string>(),
            _fixture.Create<long>(),
            _fixture.Create<long>(),
            _fixture.Create<string>(),
            new DateTime(2022, 8, 1),
            new DateTime(2022, 9, 30),
            20000,
            _fixture.Create<string>(),
            null,
            FundingType.NonLevy,
            20001,
            _fixture.Create<int>());
        _apprenticeship.CalculateEarnings();

        _result = _sut.Build(_apprenticeship);
    }

    [Test]
    public void ShouldPopulateThe_ApprenticeshipKey_Correctly()
    {
        _result.ApprenticeshipKey.Should().Be(_apprenticeship.ApprenticeshipKey);
    }

    [Test]
    public void ShouldPopulateThe_Uln_Correctly()
    {
        _result.Uln.Should().Be(_apprenticeship.Uln);
    }

    [Test]
    public void ShouldPopulateThe_EmployerId_Correctly()
    {
        _result.EmployerId.Should().Be(_apprenticeship.EmployerAccountId);
    }

    [Test]
    public void ShouldPopulateThe_ProviderId_Correctly()
    {
        _result.ProviderId.Should().Be(_apprenticeship.UKPRN);
    }

    [Test]
    public void ShouldPopulateThe_TransferSenderEmployerId_Correctly()
    {
        _result.TransferSenderEmployerId.Should().Be(_apprenticeship.FundingEmployerAccountId);
    }

    [Test]
    public void ShouldPopulateThe_AgreedPrice_Correctly()
    {
        _result.AgreedPrice.Should().Be(_apprenticeship.AgreedPrice);
    }

    [Test]
    public void ShouldPopulateThe_StartDate_Correctly()
    {
        _result.StartDate.Should().Be(_apprenticeship.ActualStartDate);
    }

    [Test]
    public void ShouldPopulateThe_TrainingCode_Correctly()
    {
        _result.TrainingCode.Should().Be(_apprenticeship.TrainingCode);
    }

    [Test]
    public void ShouldPopulateThe_EmployerType_Correctly()
    {
        _result.EmployerType.Should().Be(EmployerType.NonLevy);
    }

    [Test]
    public void ShouldBuildTheRightNumberOfDeliveryPeriods()
    {
        _result.DeliveryPeriods.Count.Should().Be(2);
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriod_Correctly()
    {
        _result.DeliveryPeriods.FirstOrDefault(x => x.Period == 1).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriod_Correctly()
    {
        _result.DeliveryPeriods.FirstOrDefault(x => x.Period == 1).Should().NotBeNull();
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriodCalendarMonth_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 1).CalendarMonth.Should().Be(8);
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriodCalendarMonth_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 2).CalendarMonth.Should().Be(9);
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriodCalendarYear_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 1).CalenderYear.Should().Be(2022);
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriodCalendarYear_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 2).CalenderYear.Should().Be(2022);
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriodAcademicYear_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 1).AcademicYear.Should().Be(2223);
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriodAcademicYear_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 2).AcademicYear.Should().Be(2223);
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriodLearningAmount_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 1).LearningAmount.Should().Be(8000);
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriodLearningAmount_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 2).LearningAmount.Should().Be(8000);
    }

    [Test]
    public void ShouldPopulateThe_FirstDeliveryPeriodFundingLineType_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 1).FundingLineType.Should().Be(_apprenticeship.FundingLineType);
    }

    [Test]
    public void ShouldPopulateThe_SecondDeliveryPeriodFundingLineType_Correctly()
    {
        _result.DeliveryPeriods.First(x => x.Period == 2).FundingLineType.Should().Be(_apprenticeship.FundingLineType);
    }

    [Test]
    public void ShouldPopulateThe_EmployerAccountId_Correctly()
    {
        _result.EmployerAccountId.Should().Be(_apprenticeship.EmployerAccountId);
    }
}