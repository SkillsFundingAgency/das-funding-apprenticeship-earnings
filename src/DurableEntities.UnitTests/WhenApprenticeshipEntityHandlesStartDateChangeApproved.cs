using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
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

public class WhenApprenticeshipEntityHandlesStartDateChangeApproved
{
    private ApprenticeshipEntity _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private ApprenticeshipStartDateChangedEvent _startDateChangedEvent;
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

        _apprenticeship = _fixture.CreateApprenticeship(new DateTime(2021, 1, 15), new DateTime(2022, 1, 15));

        _startDateChangedEvent = new ApprenticeshipStartDateChangedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 123,
            ActualStartDate = _apprenticeship.ActualStartDate.AddMonths(3),
            PlannedEndDate = _apprenticeship.PlannedEndDate,
            AgeAtStartOfApprenticeship = _fixture.Create<int>(),
            EmployerAccountId = _apprenticeshipCreatedEvent.EmployerAccountId,
            ProviderId = 123,
            ApprovedDate = new DateTime(2023, 2, 15),
            ProviderApprovedBy = "",
            EmployerApprovedBy = "",
            Initiator = ""
        };
        _apprenticeship.RecalculateEarnings(_mockSystemClock.Object, _startDateChangedEvent.ActualStartDate, _startDateChangedEvent.PlannedEndDate, _startDateChangedEvent.AgeAtStartOfApprenticeship.GetValueOrDefault());

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _approvePriceChangeCommandHandler = new Mock<IApprovePriceChangeCommandHandler>();
        _approveStartDateChangeCommandHandler = new Mock<IApproveStartDateChangeCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            
        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _approvePriceChangeCommandHandler.Object, _approveStartDateChangeCommandHandler.Object, _domainEventDispatcher.Object);
        _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);
        _approveStartDateChangeCommandHandler.Setup(x => x.RecalculateEarnings(It.IsAny<ApproveStartDateChangeCommand>())).ReturnsAsync(_apprenticeship);
        await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);

        //Act
        await _sut.HandleApprenticeshipStartDateChangeApprovedEvent(_startDateChangedEvent);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipEntityCorrectly()
    {
        _sut.Model.EarningsProfile.AdjustedPrice.Should().Be(_apprenticeship.EarningsProfile.OnProgramTotal);
        _sut.Model.EarningsProfile.CompletionPayment.Should().Be(_apprenticeship.EarningsProfile.CompletionPayment);
        _sut.Model.EarningsProfile.EarningsProfileId.Should().Be(_apprenticeship.EarningsProfile.EarningsProfileId);
        _sut.Model.EarningsProfile.Instalments.Should().BeEquivalentTo(_apprenticeship.EarningsProfile.Instalments);
        _sut.Model.AgeAtStartOfApprenticeship.Should().Be(_startDateChangedEvent.AgeAtStartOfApprenticeship);
    }

    [Test]
    public void ShouldCallRecalculateEarnings()
    {
        _approveStartDateChangeCommandHandler.Verify(x => x.RecalculateEarnings(It.Is<ApproveStartDateChangeCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
    }

    [Test]
    public void ShouldPublishEvents()
    {
        _domainEventDispatcher.Verify(x => x.Send(It.IsAny<EarningsRecalculatedEvent>(), It.IsAny<CancellationToken>()));
    }
}