using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.CreateUnapprovedApprenticeshipLearningCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.LearningController;

public class WhenCreatingUnapprovedApprenticeshipLearning
{
    private Mock<ILogger<InnerApi.Controllers.LearningController>> _loggerMock = null!;
    private Mock<ICommandDispatcher> _commandDispatcherMock = null!;
    private InnerApi.Controllers.LearningController _controller = null!;
    private Fixture _fixture = null!;

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
        var request = _fixture.Create<CreateUnapprovedApprenticeshipLearningRequest>();

        var result = await _controller.CreateUnapprovedApprenticeshipLearning(request);

        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<CreateUnapprovedApprenticeshipLearningCommand>(), default), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        var request = _fixture.Create<CreateUnapprovedApprenticeshipLearningRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<CreateUnapprovedApprenticeshipLearningCommand>(), default))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.CreateUnapprovedApprenticeshipLearning(request);

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult!.StatusCode.Should().Be(500);
    }
}
