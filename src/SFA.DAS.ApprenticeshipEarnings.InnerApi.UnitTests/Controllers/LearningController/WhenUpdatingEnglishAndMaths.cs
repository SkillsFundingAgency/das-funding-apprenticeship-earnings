using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateEnglishAndMathsCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.LearningController;

public class WhenUpdatingEnglishAndMaths
{
    private Mock<ILogger<InnerApi.Controllers.LearningController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private InnerApi.Controllers.LearningController _controller;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.LearningController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _controller = new InnerApi.Controllers.LearningController(_loggerMock.Object, _commandDispatcherMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Then_Returns_Ok_On_Success()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<UpdateEnglishAndMathsRequest>();

        // Act
        var result = await _controller.SaveMathsAndEnglish(learningKey, request);

        // Assert
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<UpdateEnglishAndMathsCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<UpdateEnglishAndMathsRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<UpdateEnglishAndMathsCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SaveMathsAndEnglish(learningKey, request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}