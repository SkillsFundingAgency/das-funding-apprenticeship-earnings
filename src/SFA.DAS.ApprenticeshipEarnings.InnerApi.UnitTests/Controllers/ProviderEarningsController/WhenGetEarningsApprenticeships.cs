using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Queries;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetFm36Data;
using SFA.DAS.Funding.ApprenticeshipEarnings.Queries.GetProviderEarningSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.InnerApi.UnitTests.Controllers.ProviderEarningsController;

public class WhenGetEarningsApprenticeships
{
    private Fixture _fixture;
    private Mock<IQueryDispatcher> _queryDispatcher;
    private InnerApi.Controllers.ProviderEarningsController _sut;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _queryDispatcher = new Mock<IQueryDispatcher>();
        _sut = new InnerApi.Controllers.ProviderEarningsController(_queryDispatcher.Object);
    }

    [Test]
    public async Task ThenTheProviderSummaryIsReturned()
    {
        var ukprn = _fixture.Create<long>();
        var year = _fixture.Create<short>();
        var period = _fixture.Create<byte>();
        var learningKey = _fixture.Create<Guid>();
        var apprenticeship = _fixture.Create<Apprenticeship>();
        var expectedResult = new GetFm36DataResponse { Apprenticeship = apprenticeship };

        _queryDispatcher.Setup(x => x.Send<GetFm36DataRequest, GetFm36DataResponse>(It.Is<GetFm36DataRequest>(r => r.Ukprn == ukprn))).ReturnsAsync(expectedResult);

        var result = await _sut.EarningsApprenticeships(ukprn, year, period, learningKey);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().Be(expectedResult);
    }
}