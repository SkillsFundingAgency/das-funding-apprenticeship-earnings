using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedShortCourseLearningCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ShortCoursesController;

public class WhenCreateUnapprovedShortCourseLearning
{
    private Mock<ILogger<InnerApi.Controllers.ShortCoursesController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private InnerApi.Controllers.ShortCoursesController _controller;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.ShortCoursesController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _controller = new InnerApi.Controllers.ShortCoursesController(_loggerMock.Object, _commandDispatcherMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Then_Returns_Ok_On_Success()
    {
        // Arrange
        var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();

        // Act
        var result = await _controller.CreateUnapprovedShortCourseLearning(request);

        // Assert
        _commandDispatcherMock.Verify(
            x => x.Send(It.IsAny<CreateUnapprovedShortCourseLearningCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var request = _fixture.Create<CreateUnapprovedShortCourseLearningRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<CreateUnapprovedShortCourseLearningCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CreateUnapprovedShortCourseLearning(request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}