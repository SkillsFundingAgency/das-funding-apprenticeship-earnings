using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure;
using Microsoft.Extensions.Internal;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;
using FundingType = SFA.DAS.Apprenticeships.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

public class WhenApprenticeshipEntityHandlesApprenticeshipCreated
{
    private ApprenticeshipEntity _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private PriceChangeApprovedEvent _priceChangeApprovedEvent;
    private Mock<ICreateApprenticeshipCommandHandler> _createApprenticeshipCommandHandler;
    private Mock<IApprovePriceChangeCommandHandler> _approvePriceChangeCommandHandler;
    private Mock<IApproveStartDateChangeCommandHandler> _approveStartDateChangeCommandHandler;
    private Mock<IDomainEventDispatcher> _domainEventDispatcher;
    private Mock<Microsoft.Extensions.Internal.ISystemClock> _mockSystemClock;
    private Fixture _fixture;
    private Apprenticeship _apprenticeship;
    

    [SetUp]
    public async Task SetUp()
    {
        _mockSystemClock = new Mock<Microsoft.Extensions.Internal.ISystemClock>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));

        _fixture = new Fixture();

        _apprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15), new DateTime(2022, 1, 15));

        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            FundingType = _apprenticeship.ApprenticeshipEpisodes.First().FundingType,
            ActualStartDate = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().ActualStartDate,
            ApprenticeshipKey = _apprenticeship.ApprenticeshipKey,
            EmployerAccountId = _apprenticeship.ApprenticeshipEpisodes.First().EmployerAccountId,
            PlannedEndDate = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().PlannedEndDate,
            UKPRN = _apprenticeship.ApprenticeshipEpisodes.First().UKPRN,
            TrainingCode = _apprenticeship.ApprenticeshipEpisodes.First().TrainingCode,
            FundingEmployerAccountId = _apprenticeship.ApprenticeshipEpisodes.First().FundingEmployerAccountId,
            Uln = _apprenticeship.Uln,
            AgreedPrice = _apprenticeship.ApprenticeshipEpisodes.First().Prices.First().AgreedPrice,
            ApprovalsApprenticeshipId = _apprenticeship.ApprovalsApprenticeshipId,
            LegalEntityName = _apprenticeship.ApprenticeshipEpisodes.First().LegalEntityName,
            AgeAtStartOfApprenticeship = _apprenticeship.ApprenticeshipEpisodes.First().AgeAtStartOfApprenticeship, //todo I think this should be on the apprenticeship
            FundingBandMaximum = (int)_apprenticeship.ApprenticeshipEpisodes.First().Prices.First().FundingBandMaximum
        };

        _apprenticeship.CalculateEarnings(_mockSystemClock.Object);

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _approvePriceChangeCommandHandler = new Mock<IApprovePriceChangeCommandHandler>();
        _approveStartDateChangeCommandHandler = new Mock<IApproveStartDateChangeCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            
        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _approvePriceChangeCommandHandler.Object, _approveStartDateChangeCommandHandler.Object, _domainEventDispatcher.Object);
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
        apprenticeshipEpisode.UKPRN.Should().Be(_apprenticeshipCreatedEvent.UKPRN);
        apprenticeshipEpisode.EmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.EmployerAccountId);
        apprenticeshipEpisode.TrainingCode.Should().Be(_apprenticeshipCreatedEvent.TrainingCode);
        apprenticeshipEpisode.FundingType.Should().Be(_apprenticeshipCreatedEvent.FundingType);
        apprenticeshipEpisode.LegalEntityName.Should().Be(_apprenticeshipCreatedEvent.LegalEntityName);
        apprenticeshipEpisode.AgeAtStartOfApprenticeship.Should().Be(_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship);
        apprenticeshipEpisode.FundingEmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.FundingEmployerAccountId);
        //todo add assertion here when ApprenticeshipEpisodeKey is available on the event from das-apprenticeships

        var expectedEpisode = _apprenticeship.ApprenticeshipEpisodes.Single();
        apprenticeshipEpisode.EarningsProfile.AdjustedPrice.Should().Be(expectedEpisode.EarningsProfile.OnProgramTotal);
        apprenticeshipEpisode.EarningsProfile.CompletionPayment.Should().Be(expectedEpisode.EarningsProfile.CompletionPayment);
        apprenticeshipEpisode.EarningsProfile.EarningsProfileId.Should().Be(expectedEpisode.EarningsProfile.EarningsProfileId);
        apprenticeshipEpisode.EarningsProfile.Instalments.Should().BeEquivalentTo(expectedEpisode.EarningsProfile.Instalments);

        apprenticeshipEpisode.Prices.Should().ContainSingle(x =>
            x.ActualStartDate == _apprenticeshipCreatedEvent.ActualStartDate
            && x.AgreedPrice == _apprenticeshipCreatedEvent.AgreedPrice
            && x.FundingBandMaximum == _apprenticeshipCreatedEvent.FundingBandMaximum
            && x.PlannedEndDate == _apprenticeshipCreatedEvent.PlannedEndDate);
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