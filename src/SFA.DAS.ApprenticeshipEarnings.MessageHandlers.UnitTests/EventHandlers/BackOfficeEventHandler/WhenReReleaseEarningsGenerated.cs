using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net.Http;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.UnitTests.EventHandlers.BackOfficeEventHandler;


[TestFixture]
public class WhenReReleaseEarningsGenerated
{
    private Mock<ILogger> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private MessageHandlers.BackOfficeEventHandler _eventHandler;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _eventHandler = new MessageHandlers.BackOfficeEventHandler(_commandDispatcherMock.Object);
    }

    [Test]
    public async Task ReReleaseEarningsGenerated_ShouldReturnOkResult_WhenCommandIsSuccessful()
    {
        // Arrange
        var ukprn = 12345678L;
        var requestData = new HttpRequestMessage();

        // Act
        var result = await _eventHandler.ReReleaseEarningsGenerated(requestData, ukprn, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default), Times.Once);
    }

    [Test]
    public async Task ReReleaseEarningsGenerated_ShouldReturnBadRequest_WhenParametersInvalid()
    {
        // Arrange
        var ukprn = 0;
        var requestData = new HttpRequestMessage();

        // Act
        var result = await _eventHandler.ReReleaseEarningsGenerated(requestData, ukprn, _loggerMock.Object);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default), Times.Never);
    }

    [Test]
    public async Task ReReleaseEarningsGenerated_ShouldReturnException_WhenExceptionIsThrown()
    {
        // Arrange
        var ukprn = 12345678L;
        var requestData = new HttpRequestMessage();
        _commandDispatcherMock.Setup(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = _eventHandler.ReReleaseEarningsGenerated(requestData, ukprn, _loggerMock.Object);
        Action act = () => result.GetAwaiter().GetResult();

        // Assert
        act.Should().Throw<Exception>();
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default), Times.Once);
    }
}

