using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SavePricesCommand;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ApprenticeshipController;

public class WhenSavePrices
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
        var request = _fixture.Create<SavePricesRequest>();

        // Act
        var result = await _controller.SavePrices(learningKey, request);

        // Assert
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<SavePricesCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<SavePricesRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<SavePricesCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SavePrices(learningKey, request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}