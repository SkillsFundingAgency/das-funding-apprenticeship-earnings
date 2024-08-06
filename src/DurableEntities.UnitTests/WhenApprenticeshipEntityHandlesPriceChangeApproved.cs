using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessUpdatedEpisodeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using ApprenticeshipEpisode = SFA.DAS.Apprenticeships.Types.ApprenticeshipEpisode;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

public class WhenApprenticeshipEntityHandlesPriceChangeApproved
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

        _apprenticeshipPriceChangedEvent = new ApprenticeshipPriceChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 120,
            ApprovedBy = ApprovedBy.Provider,
            ApprovedDate = new DateTime(2023, 2, 15),
            EffectiveFromDate = new DateTime(2023, 1, 15),
            Episode = new ApprenticeshipEpisode
            {
                Prices = new List<ApprenticeshipEpisodePrice>
                {
                    new ApprenticeshipEpisodePrice
                    {
                        TrainingPrice = 17000,
                        EndPointAssessmentPrice = 1000,
                        TotalPrice = 18000
                    }
                },
                EmployerAccountId = _apprenticeshipCreatedEvent.Episode.EmployerAccountId,
                Ukprn = 123
            }
        };

        _fixture = new Fixture();
        _apprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15),new DateTime(2022, 1, 15));
        _apprenticeship.CalculateEarnings(_mockSystemClock.Object);

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _processEpisodeUpdatedCommandHandler = new Mock<IProcessEpisodeUpdatedCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _domainEventDispatcher.Object, _processEpisodeUpdatedCommandHandler.Object);

        _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);
        _processEpisodeUpdatedCommandHandler.Setup(x => x.RecalculateEarnings(It.IsAny<ProcessEpisodeUpdatedCommand>())).ReturnsAsync(_apprenticeship);

        await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);
        await _sut.HandleApprenticeshipPriceChangeApprovedEvent(_apprenticeshipPriceChangedEvent);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipEntityCorrectly()
    {
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        var expectedEpisode = _apprenticeship.GetCurrentEpisode(_mockSystemClock.Object);

        currentEpisode.EarningsProfile.AdjustedPrice.Should().Be(expectedEpisode.EarningsProfile.OnProgramTotal);
        currentEpisode.EarningsProfile.CompletionPayment.Should().Be(expectedEpisode.EarningsProfile.CompletionPayment);
        currentEpisode.EarningsProfile.EarningsProfileId.Should().Be(expectedEpisode.EarningsProfile.EarningsProfileId);
        currentEpisode.EarningsProfile.Instalments.Should().BeEquivalentTo(expectedEpisode.EarningsProfile.Instalments);
    }

    [Test]
    public void ShouldCallRecalculateEarnings()
    {
        _processEpisodeUpdatedCommandHandler.Verify(x => x.RecalculateEarnings(It.Is<ProcessEpisodeUpdatedCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
    }
}