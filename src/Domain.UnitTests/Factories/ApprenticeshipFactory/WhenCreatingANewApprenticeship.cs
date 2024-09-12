using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;

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
        var apprenticeshipCreatedEvent = _fixture.Create<ApprenticeshipCreatedEvent>();

        var apprenticeship = _factory.CreateNew(apprenticeshipCreatedEvent);

        apprenticeshipCreatedEvent.ApprenticeshipKey.Should().Be(apprenticeship.ApprenticeshipKey);
        apprenticeshipCreatedEvent.ApprovalsApprenticeshipId.Should().Be(apprenticeship.ApprovalsApprenticeshipId);
        apprenticeshipCreatedEvent.Uln.Should().Be(apprenticeship.Uln);
        
        var episode = apprenticeship.ApprenticeshipEpisodes.SingleOrDefault(x =>
            x.UKPRN == apprenticeshipCreatedEvent.Episode.Ukprn &&
            x.EmployerAccountId == apprenticeshipCreatedEvent.Episode.EmployerAccountId &&
            x.TrainingCode == apprenticeshipCreatedEvent.Episode.TrainingCode &&
            x.LegalEntityName == apprenticeshipCreatedEvent.Episode.LegalEntityName &&
            x.AgeAtStartOfApprenticeship == apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship &&
            x.FundingEmployerAccountId == apprenticeshipCreatedEvent.Episode.FundingEmployerAccountId &&
            x.ApprenticeshipEpisodeKey == apprenticeshipCreatedEvent.Episode.Key);

        episode.Should().NotBeNull();

        var price = apprenticeshipCreatedEvent.Episode.Prices.First();
        episode.Prices.Count.Should().Be(1);
        episode.Prices.Should().ContainSingle(x =>
            x.PriceKey == price.Key &&
            x.StartDate == price.StartDate &&
            x.EndDate == price.EndDate &&
            x.AgreedPrice == price.TotalPrice &&
            x.FundingBandMaximum == price.FundingBandMaximum);
    }
}
