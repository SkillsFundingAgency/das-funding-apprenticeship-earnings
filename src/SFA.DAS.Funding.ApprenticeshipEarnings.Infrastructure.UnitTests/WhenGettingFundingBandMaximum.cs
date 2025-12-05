using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.EarningsOuterApiClient.Models;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.UnitTests;

public class WhenGettingFundingBandMaximum
{
    private Mock<IEarningsOuterApiClient> _earningOuterApiClient;
    private FundingBandMaximumService _service;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _earningOuterApiClient = new Mock<IEarningsOuterApiClient>();
        _service = new FundingBandMaximumService(_earningOuterApiClient.Object, new Mock<ILogger<FundingBandMaximumService>>().Object);
    }

    [Test]
    public async Task ThenNullIsReturnedForNullStartDate()
    {
        var courseCode = GenerateCourseCode();
        var getStandardResponse = _fixture.Create<GetStandardResponse>();
        _earningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
        var result = await _service.GetFundingBandMaximum(courseCode, null);
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenCorrectValueFromApiForPreviousFundingBandMaximumIsReturned()
    {
        var courseCode = GenerateCourseCode();
        var getStandardResponse = _fixture.Create<GetStandardResponse>();
        getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
        getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
        _earningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
        var result = await _service.GetFundingBandMaximum(courseCode, new DateTime(2022, 01, 01));
        result.Should().Be(getStandardResponse.ApprenticeshipFunding[0].MaxEmployerLevyCap);
    }

    [Test]
    public async Task ThenCorrectValueFromApiForMostRecentFundingBandMaximumIsReturned()
    {
        var courseCode = GenerateCourseCode();
        var getStandardResponse = _fixture.Create<GetStandardResponse>();
        getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
        getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
        _earningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
        var result = await _service.GetFundingBandMaximum(courseCode, new DateTime(2022, 05, 07));
        result.Should().Be(getStandardResponse.ApprenticeshipFunding[1].MaxEmployerLevyCap);
    }

    [Test]
    public async Task ThenNullIsReturnedWhenNoFundingBandForGivenDateExists()
    {
        var courseCode = GenerateCourseCode();
        var getStandardResponse = _fixture.Create<GetStandardResponse>();
        var apprenticeshipKey = Guid.NewGuid();
        var actualStartDate = new DateTime(2021, 01, 01);
        getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
        getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
        getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
        _earningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
        var result = await _service.GetFundingBandMaximum(courseCode, actualStartDate);
        result.Should().BeNull();
    }

    private string GenerateCourseCode()
    {
        var courseCode = _fixture.Create<int>();
        return courseCode.ToString();
    }
}
