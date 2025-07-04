using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Learning.Types;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests;

public class WhenReGenerateEarningsGerenatedEvent
{
    private EarningsGeneratedEventBuilder _sut;
    private EarningsGeneratedEvent _result;
    private Fixture _fixture;
    private Apprenticeship.Apprenticeship _apprenticeship;
    private Mock<ISystemClockService> _mockSystemClock;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2022, 8, 30));
        _sut = new EarningsGeneratedEventBuilder(_mockSystemClock.Object);
        _fixture = new Fixture();

        _apprenticeship = _fixture.CreateApprenticeship(
            startDate: new DateTime(2022, 8, 1),
            endDate: new DateTime(2022, 9, 30),
            agreedPrice: 20000,
            fundingType: Learning.Enums.FundingType.NonLevy);

        _apprenticeship.CalculateEarnings(_mockSystemClock.Object); 
    }

    [Test]
    public void Should_PopulateSuccessfully()
    {
        //  Act
        var result = _sut.ReGenerate(_apprenticeship);

        //  Assert
        result.LearningKey.Should().Be(_apprenticeship.LearningKey);
        result.Uln.Should().Be(_apprenticeship.Uln);
        result.EmployerId.Should().Be(_apprenticeship.LearningEpisodes.Single().EmployerAccountId);
        result.ProviderId.Should().Be(_apprenticeship.LearningEpisodes.Single().UKPRN);
        result.TransferSenderEmployerId.Should().Be(_apprenticeship.LearningEpisodes.Single().FundingEmployerAccountId);
        result.AgreedPrice.Should().Be(_apprenticeship.LearningEpisodes.Single().Prices.Single().AgreedPrice);
        result.StartDate.Should().Be(_apprenticeship.LearningEpisodes.Single().Prices.Single().StartDate);
        result.TrainingCode.Should().Be(_apprenticeship.LearningEpisodes.Single().TrainingCode);
        result.EmployerType.Should().Be(EmployerType.NonLevy);
        result.DeliveryPeriods.Count.Should().Be(2);
        result.DeliveryPeriods.FirstOrDefault(x => x.Period == 1).Should().NotBeNull();
        result.DeliveryPeriods.FirstOrDefault(x => x.Period == 1).Should().NotBeNull();
        result.DeliveryPeriods.First(x => x.Period == 1).CalendarMonth.Should().Be(8);
        result.DeliveryPeriods.First(x => x.Period == 2).CalendarMonth.Should().Be(9);
        result.DeliveryPeriods.First(x => x.Period == 1).CalenderYear.Should().Be(2022);
        result.DeliveryPeriods.First(x => x.Period == 2).CalenderYear.Should().Be(2022);
        result.DeliveryPeriods.First(x => x.Period == 1).AcademicYear.Should().Be(2223);
        result.DeliveryPeriods.First(x => x.Period == 2).AcademicYear.Should().Be(2223);
        result.DeliveryPeriods.First(x => x.Period == 1).LearningAmount.Should().Be(8000);
        result.DeliveryPeriods.First(x => x.Period == 2).LearningAmount.Should().Be(8000);
        result.EmployerAccountId.Should().Be(_apprenticeship.LearningEpisodes.Single().EmployerAccountId);
        result.PlannedEndDate.Should().Be(_apprenticeship.LearningEpisodes.Single().Prices.Single().EndDate);
        result.ApprovalsApprenticeshipId.Should().Be(_apprenticeship.ApprovalsApprenticeshipId);
        result.AgeAtStartOfLearning.Should().Be(_apprenticeship.LearningEpisodes.Single().AgeAtStartOfApprenticeship);

        var currentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        result.DeliveryPeriods.First(x => x.Period == 1).FundingLineType.Should().Be(currentEpisode.FundingLineType);
        result.DeliveryPeriods.First(x => x.Period == 2).FundingLineType.Should().Be(currentEpisode.FundingLineType);
    }

}