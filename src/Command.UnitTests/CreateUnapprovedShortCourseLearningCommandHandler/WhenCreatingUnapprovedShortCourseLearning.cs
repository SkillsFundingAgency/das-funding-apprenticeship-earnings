using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.ShortCourse;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Factories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.CreateUnapprovedShortCourseLearningCommandHandler
{
    [TestFixture]
    public class WhenCreatingUnapprovedShortCourseLearning
    {
        private readonly Fixture _fixture = new();
        private Mock<ILogger<CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommandHandler>> _mockLogger;
        private Mock<ISystemClockService> _mockSystemClock;
        private Mock<IApprenticeshipFactory> _mockFactory;
        private Mock<IApprenticeshipRepository> _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommandHandler>>();
            _mockSystemClock = new Mock<ISystemClockService>();
            _mockFactory = new Mock<IApprenticeshipFactory>();
            _mockRepository = new Mock<IApprenticeshipRepository>();

            _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2026, 02, 17));
        }

        [Test]
        public async Task ThenTheShortCourseIsCreatedAndAddedToRepository()
        {
            // Arrange
            var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();
            var command = new CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommand(request);

            var shortCourse = Domain.Models.Learning.Get(_fixture
                .Build<LearningEntity>()
                .With(x => x.ShortCourseEpisodes, new List<ShortCourseEpisodeEntity> { 
                    _fixture.Build<ShortCourseEpisodeEntity>().Create() 
                })
                .Create());

            _mockFactory
                .Setup(x => x.CreateNewShortCourse(request))
                .Returns(shortCourse);

            var sut = new CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommandHandler(
                _mockLogger.Object,
                _mockSystemClock.Object,
                _mockFactory.Object,
                _mockRepository.Object
            );

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockFactory.Verify(x => x.CreateNewShortCourse(request), Times.Once);
            _mockRepository.Verify(x => x.Add(shortCourse), Times.Once);
            _mockRepository.Verify(x => x.Update(It.IsAny<Domain.Models.Learning>()), Times.Never);
        }

        [Test]
        public async Task ThenTheExistingShortCourseIsUpdated()
        {
            // Arrange
            var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();
            var command = new CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommand(request);

            var existingShortCourse = Domain.Models.Learning.Get(_fixture
                .Build<LearningEntity>()
                .With(x => x.LearningKey, request.LearningKey)
                .With(x => x.ShortCourseEpisodes, new List<ShortCourseEpisodeEntity>
                {
                    _fixture.Build<ShortCourseEpisodeEntity>().Create()
                }).Create());

            _mockRepository
                .Setup(x => x.Get(request.LearningKey))
                .ReturnsAsync(existingShortCourse);

            var sut = new CreateUnapprovedShortCourseLearningCommand.CreateUnapprovedShortCourseLearningCommandHandler(
                _mockLogger.Object,
                _mockSystemClock.Object,
                _mockFactory.Object,
                _mockRepository.Object
            );

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(x => x.Get(request.LearningKey), Times.Once);
            _mockRepository.Verify(x => x.Update(existingShortCourse), Times.Once);

            _mockRepository.Verify(x => x.Add(It.IsAny<Domain.Models.Learning>()), Times.Never);
            _mockFactory.Verify(x => x.CreateNewShortCourse(It.IsAny<CreateUnapprovedShortCourseLearningRequest>()), Times.Never);
        }
    }
}
