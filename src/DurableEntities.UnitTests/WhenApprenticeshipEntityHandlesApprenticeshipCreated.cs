using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;
using ApprenticeshipEpisode = SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

public class WhenApprenticeshipEntityHandlesApprenticeshipCreated
{
    private ApprenticeshipEntity _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private ApprenticeshipPriceChangedEvent _apprenticeshipPriceChangedEvent;
    private Mock<ICreateApprenticeshipCommandHandler> _createApprenticeshipCommandHandler;
    private Mock<IProcessEpisodeUpdatedCommandHandler> _processEpisodeUpdatedCommandHandler;
    private Mock<IDomainEventDispatcher> _domainEventDispatcher;
    private Mock<ISystemClockService> _mockSystemClock;
    private Fixture _fixture;
    private Apprenticeship _apprenticeship;
    

    [SetUp]
    public async Task SetUp()
    {
        _mockSystemClock = new Mock<ISystemClockService>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));

        _fixture = new Fixture();

        _apprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15), new DateTime(2022, 1, 15));

        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = _apprenticeship.ApprenticeshipKey,
            Uln = _apprenticeship.Uln,
            ApprovalsApprenticeshipId = _apprenticeship.ApprovalsApprenticeshipId,
            AgeAtStartOfApprenticeship = _apprenticeship.ApprenticeshipEpisodes.First().AgeAtStartOfApprenticeship,
            Episode = new ApprenticeshipEpisode
            {
                FundingType = Enum.Parse<SFA.DAS.Apprenticeships.Enums.FundingType>(_apprenticeship.ApprenticeshipEpisodes.First().FundingType.ToString()),
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        StartDate = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().ActualStartDate,
                        EndDate = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().PlannedEndDate,
                        TotalPrice = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice,
                        FundingBandMaximum = (int)_apprenticeship.ApprenticeshipEpisodes.First().Prices.First().FundingBandMaximum
                    }
                },
                EmployerAccountId = _apprenticeship.ApprenticeshipEpisodes.First().EmployerAccountId,
                Ukprn = _apprenticeship.ApprenticeshipEpisodes.First().UKPRN,
                TrainingCode = _apprenticeship.ApprenticeshipEpisodes.First().TrainingCode,
                FundingEmployerAccountId = _apprenticeship.ApprenticeshipEpisodes.First().FundingEmployerAccountId,
                LegalEntityName = _apprenticeship.ApprenticeshipEpisodes.First().LegalEntityName,
            }
        };

        _apprenticeship.CalculateEarnings(_mockSystemClock.Object);

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _processEpisodeUpdatedCommandHandler = new Mock<IProcessEpisodeUpdatedCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            
        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _domainEventDispatcher.Object, _processEpisodeUpdatedCommandHandler.Object);
        _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);

        await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipEntityCorrectly()
    {
        _sut.Model.ApprenticeshipKey.Should().Be(_apprenticeshipCreatedEvent.ApprenticeshipKey);
        _sut.Model.Uln.Should().Be(_apprenticeshipCreatedEvent.Uln);
        _sut.Model.ApprovalsApprenticeshipId.Should().Be(_apprenticeshipCreatedEvent.ApprovalsApprenticeshipId);


        _sut.Model.ApprenticeshipEpisodes.Should().HaveCount(1);
        var apprenticeshipEpisode = _sut.Model.ApprenticeshipEpisodes.First();
        apprenticeshipEpisode.UKPRN.Should().Be(_apprenticeshipCreatedEvent.Episode.Ukprn);
        apprenticeshipEpisode.EmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.Episode.EmployerAccountId);
        apprenticeshipEpisode.TrainingCode.Should().Be(_apprenticeshipCreatedEvent.Episode.TrainingCode);
        apprenticeshipEpisode.FundingType.ToString().Should().Be(_apprenticeshipCreatedEvent.Episode.FundingType.ToString());
        apprenticeshipEpisode.LegalEntityName.Should().Be(_apprenticeshipCreatedEvent.Episode.LegalEntityName);
        apprenticeshipEpisode.AgeAtStartOfApprenticeship.Should().Be(_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship);
        apprenticeshipEpisode.FundingEmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.Episode.FundingEmployerAccountId);
        apprenticeshipEpisode.ApprenticeshipEpisodeKey.Should().Be(_apprenticeshipCreatedEvent.Episode.Key);

        var expectedEpisode = _apprenticeship.ApprenticeshipEpisodes.Single();
        apprenticeshipEpisode.EarningsProfile.AdjustedPrice.Should().Be(expectedEpisode.EarningsProfile.OnProgramTotal);
        apprenticeshipEpisode.EarningsProfile.CompletionPayment.Should().Be(expectedEpisode.EarningsProfile.CompletionPayment);
        apprenticeshipEpisode.EarningsProfile.EarningsProfileId.Should().Be(expectedEpisode.EarningsProfile.EarningsProfileId);
        apprenticeshipEpisode.EarningsProfile.Instalments.Should().BeEquivalentTo(expectedEpisode.EarningsProfile.Instalments);

        apprenticeshipEpisode.Prices.Should().ContainSingle(x =>
            x.ActualStartDate == _apprenticeshipCreatedEvent.Episode.Prices.First().StartDate
            && x.AgreedPrice == _apprenticeshipCreatedEvent.Episode.Ukprn
            && x.FundingBandMaximum == _apprenticeshipCreatedEvent.Episode.Ukprn
            && x.PlannedEndDate == _apprenticeshipCreatedEvent.Episode.Prices.First().EndDate);
    }

    [Test]
    public void ShouldCallGenerateEarnings()
    {
        _createApprenticeshipCommandHandler.Verify(x => x.Create(It.Is<CreateApprenticeshipCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
    }

    [Test]
    public void ShouldPublishEvents()
    {
        _domainEventDispatcher.Verify(x => x.Send(It.IsAny<EarningsCalculatedEvent>(), It.IsAny<CancellationToken>()));
    }
}