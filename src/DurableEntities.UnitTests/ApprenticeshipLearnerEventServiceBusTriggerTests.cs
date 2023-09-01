using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.UnitTests
{
    public class ApprenticeshipLearnerEventServiceBusTriggerTests
    {
        private readonly EarningsFunctions _sut = new();
        private readonly Fixture _fixture = new();
        private readonly Mock<IDurableEntityClient> _entityClientMock = new();
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent = null!;

        [SetUp]
        public void SetUp()
        {
            _apprenticeshipCreatedEvent = _fixture.Create<ApprenticeshipCreatedEvent>();
        }

        [Test]
        public async Task GeneratesEarningsForPilotApprenticeship()
        {
            _apprenticeshipCreatedEvent.FundingPlatform = FundingPlatform.DAS;
            await _sut.ApprenticeshipLearnerEventServiceBusTrigger(_apprenticeshipCreatedEvent, _entityClientMock.Object,
                Mock.Of<ILogger>());

            var entityId = new EntityId(nameof(ApprenticeshipEntity), _apprenticeshipCreatedEvent.ApprenticeshipKey.ToString());

            _entityClientMock.Verify(ecm =>
                ecm.SignalEntityAsync(
                    entityId,
                    "HandleApprenticeshipLearnerEvent",
                    _apprenticeshipCreatedEvent,
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ), Times.Once);
        }

        [Test]
        public async Task DoesNotGenerateEarningsForNonPilotApprenticeship()
        {
            _apprenticeshipCreatedEvent.FundingPlatform = FundingPlatform.SLD;
            await _sut.ApprenticeshipLearnerEventServiceBusTrigger(_apprenticeshipCreatedEvent, _entityClientMock.Object,
                Mock.Of<ILogger>());

            _entityClientMock.Verify(ecm => 
                ecm.SignalEntityAsync(
                    It.IsAny<EntityId>(), 
                    It.IsAny<string>(), 
                    It.IsAny<object>(),
                    It.IsAny<string>(), 
                    It.IsAny<string>()
                    ), Times.Never);
        }
    }
}