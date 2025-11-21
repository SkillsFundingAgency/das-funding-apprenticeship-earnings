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
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ProcessWithdrawMathsAndEnglishCommand;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ApprenticeshipController;

public class WhenWithdrawMathsAndEnglish
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
        var apprenticeshipKey = Guid.NewGuid();
        var request = _fixture.Create<MathsAndEnglishWithdrawRequest>();

        // Act
        var result = await _controller.WithdrawMathsAndEnglish(apprenticeshipKey, request);

        // Assert
        _commandDispatcherMock.Verify(
            x => x.Send(It.Is<ProcessWithdrawnMathsAndEnglishCommand>(c =>
                    c.ApprenticeshipKey == apprenticeshipKey &&
                    c.Course == request.Course &&
                    c.WithdrawalDate == request.WithdrawalDate),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().BeOfType<OkResult>();
    }
}