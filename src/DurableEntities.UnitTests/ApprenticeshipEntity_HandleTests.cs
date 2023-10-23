using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.PriceChangeApprovedCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests
{
    public class ApprenticeshipEntity_HandleTests
    {
        private ApprenticeshipEntity _sut;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private PriceChangeApprovedEvent _priceChangeApprovedEvent;
        private Mock<ICreateApprenticeshipCommandHandler> _createApprenticeshipCommandHandler;
        private Mock<IPriceChangeApprovedCommandHandler> _priceChangeApprovedCommandHandler;
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

            _apprenticeship = new Apprenticeship(
                Guid.NewGuid(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                new DateTime(2021, 1, 15),
                new DateTime(2022, 1, 15),
                _fixture.Create<decimal>(),
                _fixture.Create<string>(),
                null,
                _fixture.Create<FundingType>(),
                _fixture.Create<decimal>(),
                _fixture.Create<int>());
            _apprenticeship.CalculateEarnings();

            _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();
            _priceChangeApprovedCommandHandler = new Mock<IPriceChangeApprovedCommandHandler>();
            _domainEventDispatcher = new Mock<IDomainEventDispatcher>();
            _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object, _priceChangeApprovedCommandHandler.Object, _domainEventDispatcher.Object);

            _createApprenticeshipCommandHandler.Setup(x => x.Create(It.IsAny<CreateApprenticeshipCommand>())).ReturnsAsync(_apprenticeship);
            _priceChangeApprovedCommandHandler.Setup(x => x.RecalculateEarnings(It.IsAny<PriceChangeApprovedCommand>())).ReturnsAsync(_apprenticeship);

            await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);
            await _sut.HandleApprenticeshipPriceChangeApprovedEvent(_priceChangeApprovedEvent);
        }

        [Test]
        public void ShouldMapApprenticeshipKeyToEntity()
        {
            _sut.Model.ApprenticeshipKey.Should().Be(_apprenticeshipCreatedEvent.ApprenticeshipKey);
        }

        [Test]
        public void ShouldMapUlnToEntity()
        {
            _sut.Model.Uln.Should().Be(_apprenticeshipCreatedEvent.Uln);
        }

        [Test]
        public void ShouldMapUKPRNToEntity()
        {
            _sut.Model.UKPRN.Should().Be(_apprenticeshipCreatedEvent.UKPRN);
        }

        [Test]
        public void ShouldMapEmployerAccountIdToEntity()
        {
            _sut.Model.EmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.EmployerAccountId);
        }

        [Test]
        public void ShouldMapActualStartDateToEntity()
        {
            _sut.Model.ActualStartDate.Should().Be(_apprenticeshipCreatedEvent.ActualStartDate);
        }

        [Test]
        public void ShouldMapPlannedEndDateToEntity()
        {
            _sut.Model.PlannedEndDate.Should().Be(_apprenticeshipCreatedEvent.PlannedEndDate);
        }

        [Test]
        public void ShouldMapAgreedPriceToEntity()
        {
            _sut.Model.AgreedPrice.Should().Be(_apprenticeshipCreatedEvent.AgreedPrice);
        }

        [Test]
        public void ShouldMapTrainingCodeToEntity()
        {
            _sut.Model.TrainingCode.Should().Be(_apprenticeshipCreatedEvent.TrainingCode);
        }

        [Test]
        public void ShouldMapFundingEmployerAccountIdToEntity()
        {
            _sut.Model.FundingEmployerAccountId.Should().Be(_apprenticeshipCreatedEvent.FundingEmployerAccountId);
        }

        [Test]
        public void ShouldMapFundingTypeToEntity()
        {
            _sut.Model.FundingType.Should().Be(_apprenticeshipCreatedEvent.FundingType);
        }

        [Test]
        public void ShouldMapApprovalsApprenticeshipIdToEntity()
        {
            _sut.Model.ApprovalsApprenticeshipId.Should().Be(_apprenticeshipCreatedEvent.ApprovalsApprenticeshipId);
        }

        [Test]
        public void ShouldMapLegalEntityNameToEntity()
        {
            _sut.Model.LegalEntityName.Should().Be(_apprenticeshipCreatedEvent.LegalEntityName);
        }

        [Test]
        public void ShouldMapFundingBandMaximumToEntity()
        {
            _sut.Model.FundingBandMaximum.Should().Be(_apprenticeshipCreatedEvent.FundingBandMaximum);
        }
        
        [Test]
        public void ShouldMapAgeAtStartOfApprenticeshipToEntity()
        {
            _sut.Model.AgeAtStartOfApprenticeship.Should().Be(_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship);
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

        [Test]
        public void ShouldCallRegenerateEarnings()
        {
            _priceChangeApprovedCommandHandler.Verify(x => x.RecalculateEarnings(It.Is<PriceChangeApprovedCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
        }
    }
}