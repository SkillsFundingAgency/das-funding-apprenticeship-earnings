using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;
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
    private DateTime _orginalStartDate = new DateTime(2021, 1, 15);
    private DateTime _orginalEndDate = new DateTime(2021, 12, 31);

    public WhenRecalculatingEarningsForStartDateChange()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _updatedStartDate = new DateTime(2021, 3, 15);
        _updatedAgeAtApprenticeshipStart = _fixture.Create<int>();
        _apprenticeshipBeforeStartDateChange = CreateApprenticeship(_fixture.Create<decimal>(), _orginalStartDate, _orginalEndDate);
        _apprenticeshipBeforeStartDateChange.CalculateEarnings();
        _sut = CreateUpdatedApprenticeship(_apprenticeshipBeforeStartDateChange, _updatedStartDate);
    }

    [Test]
    public void ThenTheActualStartDateAndAgeAreUpdated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
        _sut.ActualStartDate.Should().Be(_updatedStartDate);
        _sut.AgeAtStartOfApprenticeship.Should().Be(_updatedAgeAtApprenticeshipStart);
    }

    [Test]
    public void ThenTheEarningsProfileIsCalculated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
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
        _sut!.RecalculateEarnings(_updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
    }
        
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut!.RecalculateEarnings(_updatedStartDate, _orginalEndDate, _updatedAgeAtApprenticeshipStart);
        _sut.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
    }

    private Apprenticeship.Apprenticeship CreateApprenticeship(decimal agreedPrice, DateTime startDate, DateTime endDate)
    {
        var apprenticeshipEntityModel = _fixture.Create<ApprenticeshipEntityModel>();
        apprenticeshipEntityModel.ActualStartDate = startDate;
        apprenticeshipEntityModel.PlannedEndDate = endDate;
        apprenticeshipEntityModel.AgreedPrice = agreedPrice;
        apprenticeshipEntityModel.FundingBandMaximum = agreedPrice + 1;
        apprenticeshipEntityModel.FundingEmployerAccountId = null;

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }
 
    private Apprenticeship.Apprenticeship CreateUpdatedApprenticeship(Apprenticeship.Apprenticeship apprenticeship, DateTime newStartDate)
    {
        var apprenticeshipEntityModel = _fixture.Create<ApprenticeshipEntityModel>();

        apprenticeshipEntityModel.ApprenticeshipKey = apprenticeship.ApprenticeshipKey;
        apprenticeshipEntityModel.ApprovalsApprenticeshipId = apprenticeship.ApprovalsApprenticeshipId;
        apprenticeshipEntityModel.Uln = apprenticeship.Uln;
        apprenticeshipEntityModel.ApprenticeshipEpisodes = apprenticeship.ApprenticeshipEpisodes.Select(x => new ApprenticeshipEpisodeModel { UKPRN = x.UKPRN }).ToList();
        apprenticeshipEntityModel.EmployerAccountId = apprenticeship.EmployerAccountId;
        apprenticeshipEntityModel.LegalEntityName = apprenticeship.LegalEntityName;
        apprenticeshipEntityModel.ActualStartDate = newStartDate;
        apprenticeshipEntityModel.PlannedEndDate = apprenticeship.PlannedEndDate;
        apprenticeshipEntityModel.AgreedPrice = apprenticeship.AgreedPrice;
        apprenticeshipEntityModel.TrainingCode = apprenticeship.TrainingCode;
        apprenticeshipEntityModel.FundingEmployerAccountId = apprenticeship.FundingEmployerAccountId;
        apprenticeshipEntityModel.FundingType = apprenticeship.FundingType;
        apprenticeshipEntityModel.FundingBandMaximum = apprenticeship.AgreedPrice + 1;
        apprenticeshipEntityModel.AgeAtStartOfApprenticeship = apprenticeship.AgeAtStartOfApprenticeship;

        apprenticeshipEntityModel.EarningsProfile = MapEarningsProfileToModel(apprenticeship.EarningsProfile);

        return new Apprenticeship.Apprenticeship(apprenticeshipEntityModel);
    }

    internal static EarningsProfileEntityModel MapEarningsProfileToModel(EarningsProfile earningsProfile)
    {
        var instalments = earningsProfile.Instalments.Select(i => new InstalmentEntityModel
        {
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount
        }).ToList();

        return new EarningsProfileEntityModel
        {
            AdjustedPrice = earningsProfile.OnProgramTotal,
            Instalments = instalments,
            CompletionPayment = earningsProfile.CompletionPayment,
            EarningsProfileId = earningsProfile.EarningsProfileId
        };
    }

}