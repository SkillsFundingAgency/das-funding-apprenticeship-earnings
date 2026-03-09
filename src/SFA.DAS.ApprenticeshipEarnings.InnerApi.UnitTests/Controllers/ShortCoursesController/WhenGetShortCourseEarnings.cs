using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetShortCourseEarnings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ShortCoursesController;

public class WhenGetShortCourseEarnings
{
    private Mock<ILogger<InnerApi.Controllers.ShortCoursesController>> _loggerMock;
    private Mock<ICommandDispatcher> _commandDispatcherMock;
    private Mock<IQueryDispatcher> _queryDispatcherMock;
    private InnerApi.Controllers.ShortCoursesController _controller;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<InnerApi.Controllers.ShortCoursesController>>();
        _commandDispatcherMock = new Mock<ICommandDispatcher>();
        _queryDispatcherMock = new Mock<IQueryDispatcher>();
        _controller = new InnerApi.Controllers.ShortCoursesController(_loggerMock.Object, _commandDispatcherMock.Object, _queryDispatcherMock.Object);
        _fixture = new Fixture();
    }

    [Test]
    public async Task Then_Returns_Ok_With_Earnings_On_Success()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();
        var expectedResponse = new GetShortCourseEarningsResponse
        {
            Earnings = new List<GetShortCourseEarningsResponse.Earning>
            {
                new() { CollectionYear = 2021, CollectionPeriod = 7, Amount = 600, Type = "Regular" },
                new() { CollectionYear = 2021, CollectionPeriod = 11, Amount = 1400, Type = "Completion" }
            }
        };

        _queryDispatcherMock
            .Setup(x => x.Send<GetShortCourseEarningsRequest, GetShortCourseEarningsResponse>(
                It.Is<GetShortCourseEarningsRequest>(r => r.LearningKey == learningKey && r.Ukprn == ukprn)))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetShortCourseEarnings(learningKey, ukprn);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResponse);
    }

    [Test]
    public async Task Then_Returns_InternalServerError_On_Exception()
    {
        // Arrange
        var learningKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();

        _queryDispatcherMock
            .Setup(x => x.Send<GetShortCourseEarningsRequest, GetShortCourseEarningsResponse>(
                It.IsAny<GetShortCourseEarningsRequest>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetShortCourseEarnings(learningKey, ukprn);

        // Assert
        result.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)result).StatusCode.Should().Be(500);
    }
}
