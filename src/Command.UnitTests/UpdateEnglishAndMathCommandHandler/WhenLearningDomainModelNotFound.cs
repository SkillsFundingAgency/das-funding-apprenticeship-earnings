using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Models.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.UpdateEnglishAndMathCommandHandler;

[TestFixture]
public class WhenLearningDomainModelNotFound
{
    private readonly Mock<ILogger<UpdateEnglishAndMathsCommandHandler>> _mockLogger = new();
    private readonly Mock<ISystemClockService> _mockSystemClock = new();
    private readonly Mock<ILearningRepository> _mockRepository = new();

    [Test]
    public async Task Handle_ShouldReturnGracefully_WhenLearningDomainModelIsNull()
    {
        // Arrange
        var command = new UpdateEnglishAndMathsCommand.UpdateEnglishAndMathsCommand(
            Guid.NewGuid(),
            new UpdateEnglishAndMathsRequest { EnglishAndMaths = new List<EnglishAndMathsItem>() });

        _mockRepository
            .Setup(x => x.GetApprenticeshipLearning(command.LearningKey))
            .ReturnsAsync((ApprenticeshipLearning)null);

        var sut = new UpdateEnglishAndMathsCommandHandler(
            _mockLogger.Object,
            _mockRepository.Object,
            _mockSystemClock.Object
        );

        // Act
        Assert.DoesNotThrowAsync(async () => await sut.Handle(command));

        // Assert
        _mockRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearning>()), Times.Never);
    }
}
