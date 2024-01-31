using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.EarningsCalculatedHandler
{
    [TestFixture]
    public class WhenEventHandled
    {
        private Fixture _fixture;

        public WhenEventHandled()
        {
            _fixture = new Fixture();
        }

        [Test]
        public async Task ThenRepositoryIsCalled()
        {
            //  Arrange
            var repository = new Mock<IEarningsQueryRepository>();
            var sut = new Apprenticeship.Events.EarningsCalculatedHandler(repository.Object);
            var @event = _fixture.Create<EarningsCalculatedEvent>();

            //  Act
            await sut.Handle(@event);

            //  Assert
            repository.Verify(x => x.Add(@event.Apprenticeship), Times.Once);
        }
    }
}
