using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Learning.Types;
using System.Linq;

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
        var learningCreatedEvent = _fixture.CreateLearningCreatedEvent();

        var apprenticeship = _factory.CreateNew(learningCreatedEvent);

        learningCreatedEvent.LearningKey.Should().Be(apprenticeship.ApprenticeshipKey);
        learningCreatedEvent.ApprovalsApprenticeshipId.Should().Be(apprenticeship.ApprovalsApprenticeshipId);
        learningCreatedEvent.Uln.Should().Be(apprenticeship.Uln);
        
        var episode = apprenticeship.ApprenticeshipEpisodes.SingleOrDefault(x =>
            x.UKPRN == learningCreatedEvent.Episode.Ukprn &&
            x.EmployerAccountId == learningCreatedEvent.Episode.EmployerAccountId &&
            x.TrainingCode == learningCreatedEvent.Episode.TrainingCode &&
            x.LegalEntityName == learningCreatedEvent.Episode.LegalEntityName &&
            x.AgeAtStartOfApprenticeship == learningCreatedEvent.Episode.AgeAtStartOfLearning &&
            x.FundingEmployerAccountId == learningCreatedEvent.Episode.FundingEmployerAccountId &&
            x.ApprenticeshipEpisodeKey == learningCreatedEvent.Episode.Key);

        episode.Should().NotBeNull();

        var price = learningCreatedEvent.Episode.Prices.First();
        episode.Prices.Count.Should().Be(1);
        episode.Prices.Should().ContainSingle(x =>
            x.PriceKey == price.Key &&
            x.StartDate == price.StartDate &&
            x.EndDate == price.EndDate &&
            x.AgreedPrice == price.TotalPrice);
    }
}
