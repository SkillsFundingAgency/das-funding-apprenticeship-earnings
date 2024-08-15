using System;
using System.Collections.Generic;
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
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using ApprenticeshipEpisode = SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

public class WhenApprenticeshipEntityHandlesStartDateChangeApproved
{
    private ApprenticeshipEntity _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private ApprenticeshipStartDateChangedEvent _startDateChangedEvent;
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

        _apprenticeshipCreatedEvent = new ApprenticeshipCreatedEvent
        {
            ApprenticeshipKey = Guid.NewGuid(),
            Uln = "900000118",
            ApprovalsApprenticeshipId = 120,
            Episode = new ApprenticeshipEpisode
            {
                FundingType = Apprenticeships.Enums.FundingType.NonLevy,
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        StartDate = new DateTime(2022, 8, 1),
                        EndDate = new DateTime(2024, 7, 31),
                        TotalPrice = 15000
                    }
                },
                EmployerAccountId = 114,
                Ukprn = 116,
                TrainingCode = "able-seafarer",
                FundingEmployerAccountId = 118,
                LegalEntityName = "MyTrawler",
                AgeAtStartOfApprenticeship = 20,
            }
        };

        var apprenticeshipStartDate = new DateTime(2021, 1, 15);
        var apprenticeshipEndDate = new DateTime(2022, 1, 15);

        _apprenticeship = _fixture.CreateApprenticeship(apprenticeshipStartDate, apprenticeshipEndDate);

        _startDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            ApprovedDate = new DateTime(2023, 2, 15),
            ProviderApprovedBy = "",
            EmployerApprovedBy = "",
            Initiator = "",
            StartDate = apprenticeshipStartDate.AddMonths(3),
            Episode = new ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        StartDate = apprenticeshipStartDate.AddMonths(3),
                        EndDate = apprenticeshipEndDate
                    }
                },
                EmployerAccountId = _apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = 123,
                AgeAtStartOfApprenticeship = _fixture.Create<int>()
            }
        };
        _apprenticeship.RecalculateEarnings(_startDateChangedEvent, _mockSystemClock.Object);

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
        _processEpisodeUpdatedCommandHandler = new Mock<IProcessEpisodeUpdatedCommandHandler>();

        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _domainEventDispatcher.Object, _processEpisodeUpdatedCommandHandler.Object);
        _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);
        _processEpisodeUpdatedCommandHandler.Setup(x => x.RecalculateEarnings(It.IsAny<ProcessEpisodeUpdatedCommand>())).ReturnsAsync(_apprenticeship);
        await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);

        //Act
        await _sut.HandleApprenticeshipStartDateChangeApprovedEvent(_startDateChangedEvent);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipEntityCorrectly()
    {
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        var expectedCurrentEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        currentEpisode.EarningsProfile.AdjustedPrice.Should().Be(expectedCurrentEpisode.EarningsProfile.OnProgramTotal);
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(expectedCurrentEpisode.EarningsProfile.CompletionPayment);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().Be(expectedCurrentEpisode.EarningsProfile.EarningsProfileId);
        currentEpisode.EarningsProfile.Instalments.Should().BeEquivalentTo(expectedCurrentEpisode.EarningsProfile.Instalments);
        currentEpisode.AgeAtStartOfApprenticeship.Should().Be(expectedCurrentEpisode.AgeAtStartOfApprenticeship);
    }

    [Test]
    public void ShouldCallRecalculateEarnings()
    {
        _processEpisodeUpdatedCommandHandler.Verify(x => x.RecalculateEarnings(It.Is<ProcessEpisodeUpdatedCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
    }

    [Test]
    public void ShouldPublishEvents()
    {
        _domainEventDispatcher.Verify(x => x.Send(It.IsAny<EarningsRecalculatedEvent>(), It.IsAny<CancellationToken>()));
    }
}