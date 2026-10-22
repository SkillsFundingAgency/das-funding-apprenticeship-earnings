using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using SFA.DAS.Payments.EarningEvents.Messages.External;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenUpdatingFurtherApprovalNeeded
{
    private const string Uln = "1234567890";
    private const string TrainingCode = "614";

    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
    }

    [Test]
    public void AndItIsSetOnADasEpisodeThenTheFlagIsSetAndPaymentsAreWithheld()
    {
        var (learning, episode) = CreateLearningWithEpisode(FundingPlatform.DAS);

        episode.UpdateFurtherApprovalNeeded(learning, true);

        episode.FurtherApprovalNeeded.Should().BeTrue();
        var @event = learning.FlushEvents().OfType<WithholdPaymentsForLearningEvent>().Single();
        @event.UKPRN.Should().Be(episode.UKPRN);
        @event.ULN.Should().Be(long.Parse(Uln));
        @event.LearningKey.Should().Be(learning.LearningKey);
        @event.CourseCode.Should().Be(TrainingCode);
        @event.LearningType.Should().Be(LearningType.Apprenticeship);
        @event.CourseType.Should().Be(CourseType.Apprenticeship);
    }

    [Test]
    public void AndItIsAlreadySetThenPaymentsAreNotWithheldAgain()
    {
        var (learning, episode) = CreateLearningWithEpisode(FundingPlatform.DAS);
        episode.UpdateFurtherApprovalNeeded(learning, true);
        learning.FlushEvents();

        episode.UpdateFurtherApprovalNeeded(learning, true);

        episode.FurtherApprovalNeeded.Should().BeTrue();
        learning.FlushEvents().OfType<WithholdPaymentsForLearningEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndItIsClearedThenTheFlagIsClearedAndPaymentsAreNotWithheld()
    {
        var (learning, episode) = CreateLearningWithEpisode(FundingPlatform.DAS);
        episode.UpdateFurtherApprovalNeeded(learning, true);
        learning.FlushEvents();

        episode.UpdateFurtherApprovalNeeded(learning, false);

        episode.FurtherApprovalNeeded.Should().BeFalse();
        learning.FlushEvents().OfType<WithholdPaymentsForLearningEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndItWasNotSetAndIsStillNotNeededThenPaymentsAreNotWithheld()
    {
        var (learning, episode) = CreateLearningWithEpisode(FundingPlatform.DAS);

        episode.UpdateFurtherApprovalNeeded(learning, false);

        episode.FurtherApprovalNeeded.Should().BeFalse();
        learning.FlushEvents().OfType<WithholdPaymentsForLearningEvent>().Should().BeEmpty();
    }

    [Test]
    public void AndItIsSetOnAnSldEpisodeThenTheFlagIsSetButPaymentsAreNotWithheld()
    {
        var (learning, episode) = CreateLearningWithEpisode(FundingPlatform.SLD);

        episode.UpdateFurtherApprovalNeeded(learning, true);

        episode.FurtherApprovalNeeded.Should().BeTrue();
        learning.FlushEvents().OfType<WithholdPaymentsForLearningEvent>().Should().BeEmpty();
    }

    private (ApprenticeshipLearning, ApprenticeshipEpisode) CreateLearningWithEpisode(FundingPlatform fundingPlatform)
    {
        var learning = _fixture.CreateLearning(fundingPlatform: fundingPlatform);

        // The fixture gives a non-numeric ULN and no training code; the withhold event needs both.
        // TrainingCode is NCHAR(50) in the database, so it is padded here to prove the event trims it.
        var entity = (ApprenticeshipLearningEntity)learning.GetModel();
        entity.Uln = Uln;
        entity.Episodes.Single().TrainingCode = TrainingCode.PadRight(50);

        learning.FlushEvents();
        return (learning, learning.Episodes.Single());
    }
}
