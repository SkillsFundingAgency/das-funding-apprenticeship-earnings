using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.SaveCareDetailsCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ApprenticeshipController;

public class WhenSaveCareDetails
{
    private Mock<ILogger<InnerApi.Controllers.ApprenticeshipController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private InnerApi.Controllers.ApprenticeshipController _controller;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.ApprenticeshipController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _controller = new InnerApi.Controllers.ApprenticeshipController(_loggerMock.Object, _commandDispatcherMock.Object);
    }

    [Test]
    public async Task Then_Returns_Ok_On_Success()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = new SaveCareDetailsRequest
        {
            HasEHCP = true,
            IsCareLeaver = true,
            CareLeaverEmployerConsentGiven = true
        };

        // Act
        var result = await _controller.SaveCareDetails(learningKey, request);

        // Assert
        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<SaveCareDetailsCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var learningKey = Guid.NewGuid();
        var request = new SaveCareDetailsRequest
        {
            HasEHCP = true,
            IsCareLeaver = true,
            CareLeaverEmployerConsentGiven = true
        };

        _commandDispatcherMock.Setup(x => x.Send(It.IsAny<SaveCareDetailsCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SaveCareDetails(learningKey, request);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult.StatusCode.Should().Be(500);
    }
}

