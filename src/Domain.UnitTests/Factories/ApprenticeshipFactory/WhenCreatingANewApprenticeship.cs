using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

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
        var apprenticeshipEntityModel = _fixture.Create<ApprenticeshipEntityModel>();

        var apprenticeship = _factory.CreateNew(apprenticeshipEntityModel);

        apprenticeshipEntityModel.ApprenticeshipKey.Should().Be(apprenticeship.ApprenticeshipKey);
        apprenticeshipEntityModel.ApprovalsApprenticeshipId.Should().Be(apprenticeship.ApprovalsApprenticeshipId);
        apprenticeshipEntityModel.Uln.Should().Be(apprenticeship.Uln);

        foreach (var apprenticeshipEpisode in apprenticeshipEntityModel.ApprenticeshipEpisodes)
        {
            apprenticeship.ApprenticeshipEpisodes.Should().ContainSingle(x =>
                x.UKPRN == apprenticeshipEpisode.UKPRN &&
                x.EmployerAccountId == apprenticeshipEpisode.EmployerAccountId &&
                x.ActualStartDate == apprenticeshipEpisode.ActualStartDate &&
                x.PlannedEndDate == apprenticeshipEpisode.PlannedEndDate &&
                x.AgreedPrice == apprenticeshipEpisode.AgreedPrice &&
                x.TrainingCode == apprenticeshipEpisode.TrainingCode &&
                x.FundingBandMaximum == apprenticeshipEpisode.FundingBandMaximum &&
                x.LegalEntityName == apprenticeshipEpisode.LegalEntityName &&
                x.AgeAtStartOfApprenticeship == apprenticeshipEpisode.AgeAtStartOfApprenticeship &&
                x.FundingEmployerAccountId == apprenticeshipEpisode.FundingEmployerAccountId);
        }

    }
}
