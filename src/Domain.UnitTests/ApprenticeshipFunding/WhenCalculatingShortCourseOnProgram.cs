using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
internal class WhenCalculatingShortCourseOnProgram
{
    private readonly Fixture _fixture = new();
    private ShortCourseLearning _learning;
    private ShortCourseEpisode _episode;
    private Mock<ISystemClockService> _mockSystemClock;
    private decimal _agreedPrice;
    private long _employerAccountId;
    private long _fundingAccountId;

    [SetUp]
    public void SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2024, 12, 24));

        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 3, 31);
        
        _agreedPrice = 3000m;

        _learning = _fixture.CreateLearningWithShortCourse(startDate, endDate, _agreedPrice);
        _episode = _learning.Episodes.Single();

        _employerAccountId = _fixture.Create<long>();
        _fundingAccountId = _fixture.Create<long>();
    }

    [Test]
    public void ThenEarningsProfileIsCreatedIfNoneExists()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Should().NotBeNull();
        _episode.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    [Test]
    public void ThenInstalmentsAreGenerated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.Instalments.Should().NotBeEmpty();
    }

    [Test]
    public void ThenCompletionPaymentIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.CompletionPayment.Should().Be(_agreedPrice*0.7m);
    }

    [Test]
    public void ThenOnProgramTotalIsCalculated()
    {
        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Assert
        _episode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
    }

    [Test]
    public void ThenEarningsProfileIsUpdatedIfItAlreadyExists()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");

        var originalProfileId = _episode.EarningsProfile.EarningsProfileId;

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "updated");

        // Assert
        _episode.EarningsProfile.EarningsProfileId.Should().Be(originalProfileId);
    }

    [Test]
    public void ThenCalculationDataIsUpdatedWhenRecalculated()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "updated");

        // Assert
        _episode.EarningsProfile.CalculationData.Should().Be("updated");
    }

    [Test]
    public void WhenApproved_ThenBothMilestonesAreMarkedPayableIfRecorded()
    {
        // Arrange - milestones set, earnings calculated while unapproved
        _episode.UpdateMilestones(new List<Milestone> { Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete });
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");
        _episode.EarningsProfile.Instalments.Should().AllSatisfy(i => i.IsPayable.Should().BeFalse());

        // Act
        _episode.Approve(_employerAccountId, _fundingAccountId);

        // Assert
        _episode.EarningsProfile.Instalments.Should().AllSatisfy(i => i.IsPayable.Should().BeTrue());
    }

    [Test]
    public void WhenPreviouslyRemoved_ThenIsRemovedIsClearedAndEarningsAreRestored()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");
        _episode.Remove();
        _episode.EarningsProfile.Instalments.Should().BeEmpty();

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "reinstated");

        // Assert
        _episode.IsRemoved.Should().BeFalse();
        _episode.EarningsProfile.Instalments.Should().NotBeEmpty();
        _episode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
        _episode.EarningsProfile.CompletionPayment.Should().Be(_agreedPrice * 0.7m);
    }

    [Test]
    public void WhenApproved_MilestonesIsMarkedPayableIfRecorded()
    {
        // Arrange - only the 30% milestone achieved; clear withdrawal date so both instalments are generated
        _episode.UpdateWithdrawalDate(null);
        _episode.UpdateMilestones(new List<Milestone> { Milestone.ThirtyPercentLearningComplete });
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Act
        _episode.Approve(_employerAccountId, _fundingAccountId);

        // Assert
        _episode.EarningsProfile.Instalments.Single(i => i.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete).IsPayable.Should().BeTrue();
        _episode.EarningsProfile.Instalments.Single(i => i.Type == ShortCourseInstalmentType.LearningComplete).IsPayable.Should().BeFalse();
    }

    [Test]
    public void WhenProviderAHasNotClaimed30Percent_ThenProviderBGetsBothInstalments()
    {
        // Arrange
        var providerAEpisodeKey = _episode.EpisodeKey;
        _learning.UpdateOnProgramme(providerAEpisodeKey, null, null, new List<Milestone>(), "providerA-data");

        var providerBRequest = BuildProviderBRequest();
        _learning.AddUnapprovedEpisode(providerBRequest);

        var providerBEpisode = _learning.Episodes.Single(e => e.EpisodeKey == providerBRequest.EpisodeKey);

        // Act
        _learning.CalculateOnProgram(providerBRequest.EpisodeKey, "providerB-data");

        // Assert - both instalments present
        providerBEpisode.EarningsProfile.Instalments.Should().HaveCount(2);
        providerBEpisode.EarningsProfile.Instalments.Should().ContainSingle(i => i.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete);
        providerBEpisode.EarningsProfile.Instalments.Should().ContainSingle(i => i.Type == ShortCourseInstalmentType.LearningComplete);
        providerBEpisode.EarningsProfile.OnProgramTotal.Should().Be(_agreedPrice * 0.3m);
    }

    [Test]
    public void WhenProviderAHasClaimed30Percent_ThenProviderBGetsOnlyCompletionInstalment()
    {
        // Arrange - Provider A claimed 30% and has since withdrawn
        var providerAEpisodeKey = _episode.EpisodeKey;
        _learning.UpdateOnProgramme(providerAEpisodeKey, null, new DateTime(2024, 2, 15), new List<Milestone> { Milestone.ThirtyPercentLearningComplete }, "providerA-data");
        _learning.Approve(providerAEpisodeKey, _employerAccountId, _fundingAccountId);

        var providerBRequest = BuildProviderBRequest();
        _learning.AddUnapprovedEpisode(providerBRequest);

        var providerBEpisode = _learning.Episodes.Single(e => e.EpisodeKey == providerBRequest.EpisodeKey);

        // Act
        _learning.CalculateOnProgram(providerBRequest.EpisodeKey, "providerB-data");

        // Assert - only completion instalment
        providerBEpisode.EarningsProfile.Instalments.Should().HaveCount(1);
        providerBEpisode.EarningsProfile.Instalments.Single().Type.Should().Be(ShortCourseInstalmentType.LearningComplete);
        providerBEpisode.EarningsProfile.OnProgramTotal.Should().Be(0m);
    }

    [Test]
    public void WhenProviderAHasClaimed30Percent_AndProviderBSubmits30MilestoneToo_ThenStillNoThirtyPercentInstalment()
    {
        // Arrange - Provider A claimed 30% and has since withdrawn; Provider B also submits 30% milestone
        var providerAEpisodeKey = _episode.EpisodeKey;
        _learning.UpdateOnProgramme(providerAEpisodeKey, null, new DateTime(2024, 2, 15), new List<Milestone> { Milestone.ThirtyPercentLearningComplete }, "providerA-data");
        _learning.Approve(providerAEpisodeKey, _employerAccountId, _fundingAccountId);

        var providerBRequest = BuildProviderBRequest();
        _learning.AddUnapprovedEpisode(providerBRequest);

        var providerBEpisode = _learning.Episodes.Single(e => e.EpisodeKey == providerBRequest.EpisodeKey);

        // Act - Provider B's PUT arrives with 30% milestone recorded
        _learning.UpdateOnProgramme(providerBRequest.EpisodeKey, null, null, new List<Milestone> { Milestone.ThirtyPercentLearningComplete }, "providerB-data");

        // Assert - 30% still suppressed because Provider A claimed it
        providerBEpisode.EarningsProfile.Instalments.Should().HaveCount(1);
        providerBEpisode.EarningsProfile.Instalments.Single().Type.Should().Be(ShortCourseInstalmentType.LearningComplete);
        providerBEpisode.EarningsProfile.OnProgramTotal.Should().Be(0m);
    }

    [Test]
    public void WhenProviderBHas30PercentMilestone_ProviderAIsNotSuppressed()
    {
        // Arrange - Provider A claimed 30% and withdrew; Provider B also has the 30% milestone flag but is active (no withdrawal)
        // Verifies that Provider B's milestone cannot suppress Provider A's 30% in a recalculation
        var providerAEpisodeKey = _episode.EpisodeKey;
        _learning.UpdateOnProgramme(providerAEpisodeKey, null, new DateTime(2024, 2, 15), new List<Milestone> { Milestone.ThirtyPercentLearningComplete }, "providerA-data");

        var providerBRequest = BuildProviderBRequest();
        _learning.AddUnapprovedEpisode(providerBRequest);
        _learning.UpdateOnProgramme(providerBRequest.EpisodeKey, null, null, new List<Milestone> { Milestone.ThirtyPercentLearningComplete }, "providerB-data");

        var providerAEpisode = _learning.Episodes.Single(e => e.EpisodeKey == providerAEpisodeKey);

        // Act - Provider A's earnings are recalculated
        _learning.CalculateOnProgram(providerAEpisodeKey, "providerA-recalc");

        // Assert - Provider A still has their 30% instalment (Provider B's flag does not suppress it)
        providerAEpisode.EarningsProfile.Instalments.Should().ContainSingle(i => i.Type == ShortCourseInstalmentType.ThirtyPercentLearningComplete);
    }
    
    [Test]
    public void WhenApproved_ShortCoursePayableEarningsUpdatedDomainEventIsAddedWithCorrectAccountIds()
    {
        // Arrange - milestones set, earnings calculated while unapproved
        _episode.UpdateMilestones(new List<Milestone> { Milestone.ThirtyPercentLearningComplete, Milestone.LearningComplete });
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");
        _episode.EarningsProfile.Instalments.Should().AllSatisfy(i => i.IsPayable.Should().BeFalse());

        // Act
        _episode.Approve(_employerAccountId, _fundingAccountId);

        // Assert
        var events = _episode.FlushEvents();
        events.Should().ContainSingle(e =>
            e is ShortCoursePayableEarningsUpdatedEvent &&
            ((ShortCoursePayableEarningsUpdatedEvent)e).EmployerAccountId == _employerAccountId &&
            ((ShortCoursePayableEarningsUpdatedEvent)e).FundingAccountId == _fundingAccountId);
    }

    [Test]
    public void WhenApproved_EmployerIdsArePersistedOnEpisode()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "test-data");

        // Act
        _episode.Approve(_employerAccountId, _fundingAccountId);

        // Assert
        var episodeEntity = _learning.GetModel().Episodes.Single(e => e.Key == _episode.EpisodeKey);
        episodeEntity.EmployerAccountId.Should().Be(_employerAccountId);
        episodeEntity.FundingEmployerAccountId.Should().Be(_fundingAccountId);
    }

    [Test]
    public void WhenRecalculatedAfterApproval_ShortCoursePayableEarningsUpdatedDomainEventIsPublishedCorrectly()
    {
        // Arrange
        _episode.CalculateShortCourseOnProgram(calculationData: "initial");
        _episode.Approve(_employerAccountId, _fundingAccountId);
        _episode.FlushEvents();

        // Act
        _episode.CalculateShortCourseOnProgram(calculationData: "recalc");

        // Assert
        var events = _episode.FlushEvents();
        var @event = events.OfType<ShortCoursePayableEarningsUpdatedEvent>().Single();

        @event.LearningKey.Should().Be(_learning.LearningKey);
        @event.EpisodeKey.Should().Be(_episode.EpisodeKey);
        @event.EmployerAccountId.Should().Be(_employerAccountId);
        @event.FundingAccountId.Should().Be(_fundingAccountId);
    }

    private CreateUnapprovedShortCourseLearningRequest BuildProviderBRequest()
    {
        var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();
        request.LearningKey = _learning.LearningKey;
        request.OnProgramme.StartDate = new DateTime(2024, 1, 1);
        request.OnProgramme.ExpectedEndDate = new DateTime(2024, 3, 31);
        request.OnProgramme.TotalPrice = _agreedPrice;
        request.OnProgramme.WithdrawalDate = null;
        request.OnProgramme.Milestones = [];
        return request;
    }
}