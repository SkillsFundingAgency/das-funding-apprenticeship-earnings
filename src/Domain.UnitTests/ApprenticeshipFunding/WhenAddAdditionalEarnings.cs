using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;


[TestFixture]
public class WhenAddAdditionalEarnings
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;
    private decimal _agreedPrice;
    private DateTime _actualStartDate;
    private DateTime _plannedEndDate;

    public WhenAddAdditionalEarnings()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _mockSystemClockService = new Mock<ISystemClockService>();
        _mockSystemClockService.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));

        _agreedPrice = _fixture.Create<decimal>();
        _actualStartDate = new DateTime(2021, 1, 15);
        _plannedEndDate = new DateTime(2022, 1, 31);
    }


    [Test]
    public void AddAdditionalEarnings_ShouldAddEarnings()
    {
        // Arrange
        var additionalPayments = new List<AdditionalPayment>
        {
            new AdditionalPayment( 2021, 6, 150, new DateTime(2021, 1, 15), InstalmentTypes.LearningSupport),
            new AdditionalPayment( 2021, 7, 150, new DateTime(2021, 2, 15), InstalmentTypes.LearningSupport),
        };
        var sut = CreateApprenticeship(25);// At age of 25 no other additional payments will be present

        // Act
        sut.AddAdditionalEarnings(additionalPayments, _mockSystemClockService.Object);

        // Assert
        sut.ApprenticeshipEpisodes.First().EarningsProfile.AdditionalPayments.Count.Should().Be(2);
    }

    [Test]
    public void AddAdditionalEarnings_ShouldOnlyRemoveMatchingEarnings()
    {
        // Arrange
        var additionalPayments = new List<AdditionalPayment>
        {
            new AdditionalPayment( 2021, 6, 150, new DateTime(2021, 1, 15), InstalmentTypes.LearningSupport),
            new AdditionalPayment( 2021, 7, 150, new DateTime(2021, 2, 15), InstalmentTypes.LearningSupport),
        };
        var sut = CreateApprenticeship(17);// At age of 17 there will be 4 additional payments present
        sut.AddAdditionalEarnings(new List<AdditionalPayment>{
            new AdditionalPayment(2021, 8, 150, new DateTime(2021, 3, 15), InstalmentTypes.LearningSupport),
            new AdditionalPayment(2021, 9, 150, new DateTime(2021, 4, 15), InstalmentTypes.LearningSupport),
        }, _mockSystemClockService.Object);

        // Act
        sut.AddAdditionalEarnings(additionalPayments, _mockSystemClockService.Object);

        // Assert
        sut.ApprenticeshipEpisodes.First().EarningsProfile.AdditionalPayments.Count.Should().Be(6);
    }

    [Test]
    public void AddAdditionalEarnings_ShouldCreateHistory()
    {
        // Arrange
        var additionalPayments = new List<AdditionalPayment>
        {
            new AdditionalPayment( 2021, 6, 150, new DateTime(2021, 1, 15), InstalmentTypes.LearningSupport),
            new AdditionalPayment( 2021, 7, 150, new DateTime(2021, 2, 15), InstalmentTypes.LearningSupport),
        };
        var sut = CreateApprenticeship(25);// At age of 25 no other additional payments will be present

        // Act
        sut.AddAdditionalEarnings(additionalPayments, _mockSystemClockService.Object);

        // Assert
        sut.GetModel().Episodes.First().EarningsProfileHistory.Count.Should().Be(1);
    }

    private Apprenticeship.Apprenticeship CreateApprenticeship(byte apprenticeAge)
    {
        var sut = _fixture.CreateApprenticeship(_actualStartDate, _plannedEndDate, _agreedPrice, age: apprenticeAge);
        sut.CalculateEarnings(_mockSystemClockService.Object);
        return sut;
    }
}
