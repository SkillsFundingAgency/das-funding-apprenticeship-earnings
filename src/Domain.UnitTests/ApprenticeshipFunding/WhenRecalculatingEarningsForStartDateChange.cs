using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Internal;
using Moq;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForStartDateChange
{
    private Fixture _fixture;
    private Mock<ISystemClock> _mockSystemClock;
    private Apprenticeship.Apprenticeship? _apprenticeshipBeforeStartDateChange; //represents the apprenticeship before the start date change
    private Apprenticeship.Apprenticeship? _sut; // represents the apprenticeship after the start date change
    private DateTime _updatedStartDate;
    private int _updatedAgeAtApprenticeshipStart;
    private DateTime _orginalStartDate = new DateTime(2021, 1, 15);
    private DateTime _orginalEndDate = new DateTime(2021, 12, 31);

    public WhenRecalculatingEarningsForStartDateChange()
    {
        _fixture = new Fixture();
        _mockSystemClock = new Mock<ISystemClock>();
        _mockSystemClock.Setup(x => x.UtcNow).Returns(new DateTime(2021, 8, 30));
    }

    [SetUp]
    public void SetUp()
    {
        _updatedStartDate = new DateTime(2021, 3, 15);
        _updatedAgeAtApprenticeshipStart = _fixture.Create<int>();
        _apprenticeshipBeforeStartDateChange = _fixture.CreateApprenticeship(_orginalStartDate, _orginalEndDate, _fixture.Create<decimal>());
        _apprenticeshipBeforeStartDateChange.CalculateEarnings(_mockSystemClock.Object);
        _sut = _fixture.CreateUpdatedApprenticeship(_apprenticeshipBeforeStartDateChange, newStartDate: _updatedStartDate);
    }

    [Test]
    public void ThenTheActualStartDateAndAgeAreUpdated()
    {
        _sut!.RecalculateEarnings(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
        var currentEpisode = _sut.GetCurrentEpisode(_mockSystemClock.Object);
        currentEpisode.ActualStartDate.Should().Be(_updatedStartDate);
        _sut.AgeAtStartOfApprenticeship.Should().Be(_updatedAgeAtApprenticeshipStart);
    }

    [Test]
    public void ThenTheEarningsProfileIsCalculated()
    {
        _sut!.RecalculateEarnings(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
        _sut.EarningsProfile.OnProgramTotal.Should().Be(_apprenticeshipBeforeStartDateChange.EarningsProfile.OnProgramTotal);
        _sut.EarningsProfile.CompletionPayment.Should().Be(_apprenticeshipBeforeStartDateChange.EarningsProfile.CompletionPayment);
        _sut.EarningsProfile.EarningsProfileId.Should().NotBe(_apprenticeshipBeforeStartDateChange.EarningsProfile.EarningsProfileId);
        _sut.EarningsProfile.Instalments.Count.Should().Be(10);
            
        var sum = Math.Round(_sut.EarningsProfile.Instalments.Sum(x => x.Amount),2);     
        sum.Should().Be(_sut.EarningsProfile.OnProgramTotal);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        _sut!.RecalculateEarnings(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
    }
        
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut!.RecalculateEarnings(_mockSystemClock.Object, _updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
        _sut.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }
}