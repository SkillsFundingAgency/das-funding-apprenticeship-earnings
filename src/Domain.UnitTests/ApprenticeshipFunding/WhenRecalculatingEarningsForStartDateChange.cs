using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using System;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding;

[TestFixture]
public class WhenRecalculatingEarningsForStartDateChange
{
    private Fixture _fixture;
    private Apprenticeship.Apprenticeship? _apprenticeshipBeforeStartDateChange; //represents the apprenticeship before the start date change
    private Apprenticeship.Apprenticeship? _sut; // represents the apprenticeship after the start date change
    private DateTime _updatedStartDate;
    private int _updatedAgeAtApprenticeshipStart;

    public WhenRecalculatingEarningsForStartDateChange()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _updatedStartDate = new DateTime(2021, 3, 15);
        _updatedAgeAtApprenticeshipStart = _fixture.Create<int>();
        _apprenticeshipBeforeStartDateChange = CreateApprenticeship(_fixture.Create<decimal>(), new DateTime(2021, 1, 15), new DateTime(2021, 12, 31));
        _apprenticeshipBeforeStartDateChange.CalculateEarnings();
        _sut = CreateUpdatedApprenticeship(_apprenticeshipBeforeStartDateChange, _updatedStartDate);
    }

    [Test]
    public void ThenTheActualStartDateAndAgeAreUpdated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _updatedAgeAtApprenticeshipStart);
        _sut.ActualStartDate.Should().Be(_updatedStartDate);
        _sut.AgeAtStartOfApprenticeship.Should().Be(_updatedAgeAtApprenticeshipStart);
    }

    [Test]
    public void ThenTheEarningsProfileIsCalculated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _updatedAgeAtApprenticeshipStart);
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
        _sut!.RecalculateEarnings(_updatedStartDate, _updatedAgeAtApprenticeshipStart);

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
    }
        
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _updatedAgeAtApprenticeshipStart);
        _sut.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    private Apprenticeship.Apprenticeship CreateApprenticeship(decimal agreedPrice, DateTime startDate, DateTime endDate)
    {
        return new Apprenticeship.Apprenticeship(
            Guid.NewGuid(),
            _fixture.Create<long>(),
            _fixture.Create<string>(),
            _fixture.Create<long>(),
            _fixture.Create<long>(),
            _fixture.Create<string>(),
            startDate,
            endDate,
            agreedPrice,
            _fixture.Create<string>(),
            null,
            _fixture.Create<FundingType>(),
            agreedPrice + 1,
            _fixture.Create<int>());
    }
 
    private Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(Apprenticeship.Apprenticeship apprenticeship, DateTime newStartDate)
    {
        return new Apprenticeship.Apprenticeship(
            apprenticeship.ApprenticeshipKey,
            apprenticeship.ApprovalsApprenticeshipId,
            apprenticeship.Uln,
            apprenticeship.UKPRN,
            apprenticeship.EmployerAccountId,
            apprenticeship.LegalEntityName,
            newStartDate,
            apprenticeship.PlannedEndDate,
            apprenticeship.AgreedPrice,
            apprenticeship.TrainingCode,
            apprenticeship.FundingEmployerAccountId,
            apprenticeship.FundingType,
            apprenticeship.AgreedPrice + 1,
            apprenticeship.AgeAtStartOfApprenticeship,
            apprenticeship.EarningsProfile);
    }
}