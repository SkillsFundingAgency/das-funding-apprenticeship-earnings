using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models.GetApprenticeshipByUkprnResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.Models.GetApprenticeshipByUkprnResponse;

[TestFixture]
public class ApprenticeshipExtensionsTests
{
    private Fixture _fixture;
    private Mock<ISystemClockService> _mockSystemClockService;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _mockSystemClockService = new Mock<ISystemClockService>();
    }

    [Test]
    public void ToApprenticeshipReponse_ConvertsCorrectly()
    {
        // Arrange
        var dateTime = _fixture.Create<DateTime>();
        _mockSystemClockService.Setup(s => s.UtcNow).Returns(dateTime);

        var apprenticeship = _fixture.CreateApprenticeship(dateTime.AddMonths(-3),dateTime.AddMonths(3));

        // Act
        var result = apprenticeship.ToApprenticeshipReponse(_mockSystemClockService.Object);

        // Assert
        Assert.Multiple(() =>
        {
            result.Key.Should().Be(apprenticeship.ApprenticeshipKey);
            //Assert.That(result.Ukprn, Is.EqualTo(12345678));
            //Assert.That(result.FundingLineType, Is.EqualTo("Mainstream"));
            //Assert.That(result.Episodes, Has.Count.EqualTo(1));

            //var episode = result.Episodes.First();
            //Assert.That(episode.Key, Is.EqualTo("EpisodeKey1"));
            //Assert.That(episode.NumberOfInstalments, Is.EqualTo(2));
            //Assert.That(episode.CompletionPayment, Is.EqualTo(300));
            //Assert.That(episode.OnProgramTotal, Is.EqualTo(600));

            //var instalments = episode.Instalments;
            //Assert.That(instalments, Has.Count.EqualTo(2));
            //Assert.That(instalments[0].AcademicYear, Is.EqualTo("2021"));
            //Assert.That(instalments[0].DeliveryPeriod, Is.EqualTo(1));
            //Assert.That(instalments[0].Amount, Is.EqualTo(100));
            //Assert.That(instalments[1].AcademicYear, Is.EqualTo("2021"));
            //Assert.That(instalments[1].DeliveryPeriod, Is.EqualTo(2));
            //Assert.That(instalments[1].Amount, Is.EqualTo(200));
        });
    }

}

