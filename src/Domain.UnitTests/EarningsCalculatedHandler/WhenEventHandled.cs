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
        private Apprenticeship.Events.EarningsCalculatedHandler _sut;
        private Mock<IEarningsQueryRepository> _repository;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _repository = new Mock<IEarningsQueryRepository>();
            _sut = new Apprenticeship.Events.EarningsCalculatedHandler(_repository.Object);
        }

        [Test]
        public async Task ThenRepositoryIsCalled()
        {
            var @event = _fixture.Create<EarningsCalculatedEvent>();
            await _sut.Handle(@event);
            _repository.Verify(x => x.Add(@event.Apprenticeship), Times.Once);
        }
    }
}
