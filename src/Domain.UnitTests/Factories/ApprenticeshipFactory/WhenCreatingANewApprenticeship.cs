using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Factories.ApprenticeshipFactory;

[TestFixture]
public class WhenCreatingANewApprenticeship
{
    private Fixture _fixture;
    private Domain.Factories.ApprenticeshipFactory _factory;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _factory = new Domain.Factories.ApprenticeshipFactory();
    }

    [Test]
    public void ThenTheApprenticeshipIsCreated()
    {
        var LearningCreatedEvent = _fixture.Create<LearningCreatedEvent>();

        var apprenticeship = _factory.CreateNew(LearningCreatedEvent);

        LearningCreatedEvent.LearningKey.Should().Be(apprenticeship.ApprenticeshipKey);
        LearningCreatedEvent.ApprovalsApprenticeshipId.Should().Be(apprenticeship.ApprovalsApprenticeshipId);
        LearningCreatedEvent.Uln.Should().Be(apprenticeship.Uln);
        
        var episode = apprenticeship.ApprenticeshipEpisodes.SingleOrDefault(x =>
            x.UKPRN == LearningCreatedEvent.Episode.Ukprn &&
            x.EmployerAccountId == LearningCreatedEvent.Episode.EmployerAccountId &&
            x.TrainingCode == LearningCreatedEvent.Episode.TrainingCode &&
            x.LegalEntityName == LearningCreatedEvent.Episode.LegalEntityName &&
            x.AgeAtStartOfApprenticeship == LearningCreatedEvent.Episode.AgeAtStartOfLearning &&
            x.FundingEmployerAccountId == LearningCreatedEvent.Episode.FundingEmployerAccountId &&
            x.ApprenticeshipEpisodeKey == LearningCreatedEvent.Episode.Key);

        episode.Should().NotBeNull();

        var price = LearningCreatedEvent.Episode.Prices.First();
        episode.Prices.Count.Should().Be(1);
        episode.Prices.Should().ContainSingle(x =>
            x.PriceKey == price.Key &&
            x.StartDate == price.StartDate &&
            x.EndDate == price.EndDate &&
            x.AgreedPrice == price.TotalPrice &&
            x.FundingBandMaximum == price.FundingBandMaximum);
    }
}
