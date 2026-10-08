using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities.EnglishAndMaths;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Text.Json;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests.ArchiveEnglishAndMathsEarningsProfileCommandHandler;

[TestFixture]
public class WhenArchivingEnglishAndMathsEarningsProfile
{
    private readonly Fixture _fixture = new();
    private Mock<IEarningsProfileHistoryRepository> _mockRepository;
    private Mock<ILogger<ArchiveEnglishAndMathsEarningsProfileCommand.ArchiveEnglishAndMathsEarningsProfileCommandHandler>> _mockLogger;

    [SetUp]
    public void Setup()
    {
        _mockRepository = new Mock<IEarningsProfileHistoryRepository>();
        _mockLogger = new Mock<ILogger<ArchiveEnglishAndMathsEarningsProfileCommand.ArchiveEnglishAndMathsEarningsProfileCommandHandler>>();
    }

    [Test]
    public async Task Handle_ShouldAddHistoryForEnglishAndMathsCourse()
    {
        // Arrange
        var updatedEvent = _fixture.Create<EnglishAndMathsEarningsProfileUpdatedEvent>();

        EnglishAndMathsEarningsProfileHistoryEntity history = null!;
        _mockRepository
            .Setup(x => x.Add(It.IsAny<EnglishAndMathsEarningsProfileHistoryEntity>()))
            .Callback<EnglishAndMathsEarningsProfileHistoryEntity>(x => history = x);

        var sut = new ArchiveEnglishAndMathsEarningsProfileCommand.ArchiveEnglishAndMathsEarningsProfileCommandHandler(_mockRepository.Object, _mockLogger.Object);

        // Act
        await sut.Handle(new ArchiveEnglishAndMathsEarningsProfileCommand.ArchiveEnglishAndMathsEarningsProfileCommand(updatedEvent));

        // Assert
        _mockRepository.Verify(x => x.Add(It.IsAny<EnglishAndMathsEarningsProfileHistoryEntity>()), Times.Once);
        history.Key.Should().NotBeEmpty();
        history.EnglishAndMathsKey.Should().Be(updatedEvent.EnglishAndMathsKey);
        history.EarningsProfileId.Should().Be(updatedEvent.EarningsProfileId);
        history.Version.Should().Be(updatedEvent.Version);
        JsonSerializer.Deserialize<EnglishAndMaths>(history.State).Should().BeEquivalentTo(updatedEvent.EnglishAndMaths);
    }
}
