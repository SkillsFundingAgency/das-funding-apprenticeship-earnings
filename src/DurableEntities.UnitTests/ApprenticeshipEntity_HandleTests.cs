using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateApprenticeshipCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests
{
    public class ApprenticeshipEntity_HandleTests
    {
        private ApprenticeshipEntity _sut;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private Mock<ICreateApprenticeshipCommandHandler> _createApprenticeshipCommandHandler;

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
                Uln = 900000118,
                AgreedPrice = 15000,
                ApprovalsApprenticeshipId = 120,
                LegalEntityName = "MyTrawler"
            };

            _createApprenticeshipCommandHandler = new Mock<ICreateApprenticeshipCommandHandler>();

            _sut = new ApprenticeshipEntity(_createApprenticeshipCommandHandler.Object);
            await _sut.HandleApprenticeshipLearnerEvent(_apprenticeshipCreatedEvent);
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
        public void ShouldCallGenerateEarnings()
        {
            _createApprenticeshipCommandHandler.Verify(x => x.Create(It.Is<CreateApprenticeshipCommand>(y => y.ApprenticeshipEntity == _sut.Model)));
        }
        
    }
}