using System;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenApplyingFurtherApproval
{
    private const string LearnerRef = "LEARNER-REF";

    private Fixture _fixture;
    private ApprenticeshipLearning _learning;
    private ApprenticeshipEpisode _episode;
    private Guid _learnerKey;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _learning = _fixture.CreateLearning(fundingPlatform: FundingPlatform.DAS);
        _episode = _learning.Episodes.Single();
        _learnerKey = Guid.NewGuid();
    }

    [Test]
    public void AndTheFlagIsSetThenItIsClearedAndEarningsAreReleased()
    {
        ((ApprenticeshipLearningEntity)_learning.GetModel()).Episodes.Single().FurtherApprovalNeeded = true;

        _learning.ApplyFurtherApproval(_episode.EpisodeKey, _learnerKey, LearnerRef);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        var @event = _learning.FlushEvents().OfType<ApprenticeshipPayableEarningsUpdatedEvent>().Single();
        @event.LearningKey.Should().Be(_learning.LearningKey);
        @event.EpisodeKey.Should().Be(_episode.EpisodeKey);
        @event.LearnerKey.Should().Be(_learnerKey);
        @event.LearnerRef.Should().Be(LearnerRef);
    }

    [Test]
    public void AndTheFlagIsAlreadyClearThenNothingIsReleased()
    {
        _learning.ApplyFurtherApproval(_episode.EpisodeKey, _learnerKey, LearnerRef);

        _episode.FurtherApprovalNeeded.Should().BeFalse();
        _learning.FlushEvents().OfType<ApprenticeshipPayableEarningsUpdatedEvent>().Should().BeEmpty();
    }
}
