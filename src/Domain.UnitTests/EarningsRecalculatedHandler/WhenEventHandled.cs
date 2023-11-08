using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.EarningsRecalculatedHandler
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
            var sut = new Apprenticeship.Events.EarningsRecalculatedHandler(repository.Object);
            var @event = _fixture.Create<EarningsRecalculatedEvent>();

            //  Act
            await sut.Handle(@event);

            //  Assert
            repository.Verify(x => x.Replace(@event.Apprenticeship), Times.Once);
        }
    }
}
