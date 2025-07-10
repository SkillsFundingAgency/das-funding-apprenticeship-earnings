using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveMathsAndEnglishCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ApprenticeshipController;

public class WhenSaveMathsAndEnglish
{
    private Mock<ILogger<InnerApi.Controllers.ApprenticeshipController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private InnerApi.Controllers.ApprenticeshipController _controller;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.ApprenticeshipController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _controller = new InnerApi.Controllers.ApprenticeshipController(_loggerMock.Object, _commandDispatcherMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Then_Returns_Ok_On_Success()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<SaveMathsAndEnglishRequest>();

        // Act
        var result = await _controller.SaveMathsAndEnglish(learningKey, request);

        // Assert
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<SaveMathsAndEnglishCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<SaveMathsAndEnglishRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<SaveMathsAndEnglishCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SaveMathsAndEnglish(learningKey, request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}