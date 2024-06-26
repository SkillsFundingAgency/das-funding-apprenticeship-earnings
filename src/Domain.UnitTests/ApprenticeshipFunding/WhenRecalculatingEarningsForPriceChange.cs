using AutoFixture;
using FluentAssertions;
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
public class WhenRecalculatingEarningsForPriceChange
{
    private Fixture _fixture;
    private Apprenticeship.Apprenticeship? _existingApprenticeship; //represents the apprenticeship before the price change
    private Apprenticeship.Apprenticeship? _sut; // represents the apprenticeship after the price change
    private decimal _originalPrice;
    private decimal _updatedPrice;

    public WhenRecalculatingEarningsForPriceChange()
    {
        _fixture = new Fixture();
    }

    [SetUp]
    public void SetUp()
    {
        _originalPrice = _fixture.Create<decimal>();
        _updatedPrice = _fixture.Create<decimal>();
        _existingApprenticeship = CreateApprenticeship(_originalPrice, new DateTime(2021, 1, 15), new DateTime(2021, 12, 31));
        _existingApprenticeship.CalculateEarnings();
        _sut = _fixture.CreateUpdatedApprenticeship(_existingApprenticeship, newPrice: _updatedPrice);
    }

    [Test]
    public void ThenTheAgreedPriceIsUpdated()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));
        _sut.AgreedPrice.Should().Be(_updatedPrice);
    }

    [Test]
    public void ThenTheOnProgramTotalIsCalculated()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));
        _sut.EarningsProfile.OnProgramTotal.Should().Be(_updatedPrice * .8m);
    }

    [Test]
    public void ThenTheCompletionAmountIsCalculated()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));
        _sut.EarningsProfile.CompletionPayment.Should().Be(_updatedPrice * .2m);
    }

    [Test]
    public void ThenTheSumOfTheInstalmentsMatchTheOnProgramTotal()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));

        _sut.EarningsProfile.Instalments.Count.Should().Be(12);
        var sum = Math.Round(_sut.EarningsProfile.Instalments.Sum(x => x.Amount),2);     
        sum.Should().Be(_sut.EarningsProfile.OnProgramTotal);
    }

    [Test]
    public void ThenEarningsRecalculatedEventIsCreated()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));

        var events = _sut.FlushEvents();
        events.Should().ContainSingle(x => x.GetType() == typeof(EarningsRecalculatedEvent));
    }
        
    [Test]
    public void ThenTheEarningsProfileIdIsGenerated()
    {
        _sut!.RecalculateEarnings(_updatedPrice, new DateTime(2021, 6, 15));
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
 

}