using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenDeletingShortCourseEpisode
{
    private readonly Fixture _fixture = new();
    private ShortCourseLearning _learning;
    private ShortCourseEpisode _episode;
    private long _employerAccountId;
    private long _fundingAccountId;

    [SetUp]
    public void SetUp()
    {
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 3, 31);

        _learning = _fixture.CreateLearningWithShortCourse(startDate, endDate, 3000m);
        _episode = _learning.Episodes.Single();
        _episode.UpdateWithdrawalDate(null);
        _episode.CalculateShortCourseOnProgram("initial");
        _employerAccountId = _fixture.Create<long>();
        _fundingAccountId = _fixture.Create<long>();
    }

    [Test]
    public void ThenIsRemovedIsTrue()
    {
        _episode.Remove();

        _episode.IsRemoved.Should().BeTrue();
    }

    [Test]
    public void ThenInstalmentsAreRemoved()
    {
        _episode.Remove();

        _episode.EarningsProfile.Instalments.Should().BeEmpty();
    }

    [Test]
    public void ThenOnProgramTotalIsZero()
    {
        _episode.Remove();

        _episode.EarningsProfile.OnProgramTotal.Should().Be(0m);
    }

    [Test]
    public void ThenCompletionPaymentIsZero()
    {
        _episode.Remove();

        _episode.EarningsProfile.CompletionPayment.Should().Be(0m);
    }

    [Test]
    public void ThenShortCoursePayableEarningsUpdatedDomainEventIsPublishedCorrectly()
    {
        // Arrange
        _episode.Approve(_employerAccountId, _fundingAccountId);
        _episode.FlushEvents();

        // Act
        _episode.Remove();

        // Assert
        var events = _episode.FlushEvents();
        var @event = events.OfType<ShortCoursePayableEarningsUpdatedEvent>().Single();

        @event.LearningKey.Should().Be(_learning.LearningKey);
        @event.EpisodeKey.Should().Be(_episode.EpisodeKey);
        @event.EmployerAccountId.Should().Be(_employerAccountId);
        @event.FundingAccountId.Should().Be(_fundingAccountId);
    }
}
