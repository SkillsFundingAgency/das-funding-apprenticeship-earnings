using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ReleaseEarningsCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.LearningController;

public class WhenReleaseEarnings
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
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<ReleaseEarningsRequest>();

        var result = await _controller.ReleaseEarnings(learningKey, request);

        _commandDispatcherMock.Verify(x => x.Send(It.IsAny<ReleaseEarningsCommand>(), CancellationToken.None), Times.Once);
        result.Should().BeOfType<OkResult>();
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        var learningKey = Guid.NewGuid();
        var request = _fixture.Create<ReleaseEarningsRequest>();

        _commandDispatcherMock
            .Setup(x => x.Send(It.IsAny<ReleaseEarningsCommand>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Test exception"));

        var result = await _controller.ReleaseEarnings(learningKey, request);

        result.Should().BeOfType<StatusCodeResult>();
        var statusCodeResult = result as StatusCodeResult;
        statusCodeResult!.StatusCode.Should().Be(500);
    }
}