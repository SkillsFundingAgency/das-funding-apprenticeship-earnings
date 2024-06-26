using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Enums;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApprovePriceChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ApproveStartDateChangeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using FundingType = SFA.DAS.Apprenticeships.Types.FundingType;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests;

public class WhenApprenticeshipEntityHandlesPriceChangeApproved
{
    private ApprenticeshipEntity _sut;
    private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
    private PriceChangeApprovedEvent _priceChangeApprovedEvent;
    private Mock<ICreateApprenticeshipCommandHandler> _createApprenticeshipCommandHandler;
    private Mock<IApprovePriceChangeCommandHandler> _approvePriceChangeCommandHandler;
    private Mock<IApproveStartDateChangeCommandHandler> _approveStartDateChangeCommandHandler;
    private Mock<IDomainEventDispatcher> _domainEventDispatcher;
    private Fixture _fixture;
    private Apprenticeship _apprenticeship;

    [SetUp]
    public async Task SetUp()
    {
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

        _priceChangeApprovedEvent = new PriceChangeApprovedEvent
        {
            ApprenticeshipKey = _apprenticeshipCreatedEvent.ApprenticeshipKey,
            ApprenticeshipId = 120,
            TrainingPrice = 17000,
            ApprovedBy = ApprovedBy.Provider,
            ApprovedDate = new DateTime(2023, 2, 15),
            EffectiveFromDate = new DateTime(2023, 1, 15),
            EmployerAccountId = _apprenticeshipCreatedEvent.EmployerAccountId,
            AssessmentPrice = 1000,
            ProviderId = 123
        };

        _fixture = new Fixture();
        var apprenticeshipEntityModel = _fixture.Create<ApprenticeshipEntityModel>();
        apprenticeshipEntityModel.ActualStartDate = new DateTime(2021, 1, 15);
        apprenticeshipEntityModel.PlannedEndDate = new DateTime(2022, 1, 15);
        apprenticeshipEntityModel.FundingEmployerAccountId = null;
        _apprenticeship = new Apprenticeship(apprenticeshipEntityModel);
        _apprenticeship.CalculateEarnings();

        _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
        _approvePriceChangeCommandHandler = new Mock<IApprovePriceChangeCommandHandler>();
        _approveStartDateChangeCommandHandler = new Mock<IApproveStartDateChangeCommandHandler>();
        _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
        _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _approvePriceChangeCommandHandler.Object, _approveStartDateChangeCommandHandler.Object, _domainEventDispatcher.Object);

        _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);
        _approvePriceChangeCommandHandler.Setup(x => x.RecalculateEarnings(It.IsAny<ApprovePriceChangeCommand>())).ReturnsAsync(_apprenticeship);

        await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);
        await _sut.HandleApprenticeshipPriceChangeApprovedEvent(_priceChangeApprovedEvent);
    }

    [Test]
    public void ShouldPopulateTheApprenticeshipEntityCorrectly()
    {
        _sut.Model.EarningsProfile.AdjustedPrice.Should().Be(_apprenticeship.EarningsProfile.OnProgramTotal);
        _sut.Model.EarningsProfile.CompletionPayment.Should().Be(_apprenticeship.EarningsProfile.CompletionPayment);
        _sut.Model.EarningsProfile.EarningsProfileId.Should().Be(_apprenticeship.EarningsProfile.EarningsProfileId);
        _sut.Model.EarningsProfile.Instalments.Should().BeEquivalentTo(_apprenticeship.EarningsProfile.Instalments);
    }

    [Test]
    public void ShouldCallRecalculateEarnings()
    {
        _approvePriceChangeCommandHandler.Verify(x => x.RecalculateEarnings(It.Is<ApprovePriceChangeCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
    }
}