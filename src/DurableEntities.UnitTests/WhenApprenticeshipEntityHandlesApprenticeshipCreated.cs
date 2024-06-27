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

        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            FundingType = FundingType.NonLevy,
            ActualStartDate = new DateTime(2022, 8, 1),
            ApprenticeshipKey = Guid.NewGuid(),
            EmployerAccountId = 114,
            PlannedEndDate = new DateTime(2024, 7, 31),
            UKPRN = 116,
            TrainingCode = "able-seafarer",
            FundingEmployerAccountId = 118,
            Uln = "900000118",
            AgreedPrice = 15000,
            ApprovalsApprenticeshipId = 120,
            LegalEntityName = "MyTrawler",
            AgeAtStartOfApprenticeship = 20
        };

        _fixture = new Fixture();

        _apprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15), new DateTime(2022, 1, 15));
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
        _sut.Model.FundingEmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.FundingEmployerAccountId);
        _sut.Model.ApprovalsApprenticeshipId.Should().Be(_apprenticeshipCreatedEvent.ApprovalsApprenticeshipId);
        _sut.Model.LegalEntityName.Should().Be(_apprenticeshipCreatedEvent.LegalEntityName);
        
        _sut.Model.AgeAtStartOfApprenticeship.Should().Be(_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship);

        _sut.Model.EarningsProfile.AdjustedPrice.Should().Be(_apprenticeship.EarningsProfile.OnProgramTotal);
        _sut.Model.EarningsProfile.CompletionPayment.Should().Be(_apprenticeship.EarningsProfile.CompletionPayment);
        _sut.Model.EarningsProfile.EarningsProfileId.Should().Be(_apprenticeship.EarningsProfile.EarningsProfileId);
        _sut.Model.EarningsProfile.Instalments.Should().BeEquivalentTo(_apprenticeship.EarningsProfile.Instalments);

        _sut.Model.ApprenticeshipEpisodes.Should().HaveCount(1);
        var apprenticeshipEpisode = _sut.Model.ApprenticeshipEpisodes.First();
        apprenticeshipEpisode.UKPRN.Should().Be(_apprenticeshipCreatedEvent.UKPRN);
        apprenticeshipEpisode.EmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.EmployerAccountId);
        apprenticeshipEpisode.ActualStartDate.Should().Be(_apprenticeshipCreatedEvent.ActualStartDate);
        apprenticeshipEpisode.PlannedEndDate.Should().Be(_apprenticeshipCreatedEvent.PlannedEndDate);
        apprenticeshipEpisode.TrainingCode.Should().Be(_apprenticeshipCreatedEvent.TrainingCode);
        apprenticeshipEpisode.FundingType.Should().Be(_apprenticeshipCreatedEvent.FundingType);
        apprenticeshipEpisode.FundingBandMaximum.Should().Be(_apprenticeshipCreatedEvent.FundingBandMaximum);
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