using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReReleaseEarningsGeneratedCommand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.BackOfficeController;


[TestFixture]
public class WhenReReleaseEarningsGenerated
{
    private Mock<ILogger<InnerApi.Controllers.BackOfficeController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private InnerApi.Controllers.BackOfficeController _controller;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.BackOfficeController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _controller = new InnerApi.Controllers.BackOfficeController(_loggerMock.Object, _commandDispatcherMock.Object);
    }

    [Test]
    public async Task ReReleaseEarningsGenerated_ShouldReturnOkResult_WhenCommandIsSuccessful()
    {
        // Arrange
        var ukprn = 12345678L;

        // Act
        var result = await _controller.ReReleaseEarningsGenerated(ukprn);

        // Assert
        result.Should().BeOfType<OkResult>();
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default), Times.Once);
    }

    [Test]
    public async Task ReReleaseEarningsGenerated_ShouldReturnStatusCode418_WhenExceptionIsThrown()
    {
        // Arrange
        var ukprn = 12345678L;
        _commandDispatcherMock.Setup(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ReReleaseEarningsGenerated(ukprn);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(418);
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReReleaseEarningsGeneratedCommand>(), default), Times.Once);
    }
}

