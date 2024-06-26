using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding
{
    [TestFixture]
    public class WhenCalculateEarnings
    {
        private Fixture _fixture;
        private Apprenticeship.Apprenticeship _sut;

        public WhenCalculateEarnings()
        {
            _fixture = new Fixture();
        }

        [SetUp]
        public void SetUp()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var actualStartDate = new DateTime(2021, 1, 15);
            var plannedEndDate = new DateTime(2021, 12, 31);
            _sut = _fixture.CreateApprenticeship(actualStartDate, plannedEndDate, agreedPrice);
        }

        [Test]
        public void ThenTheOnProgramTotalIsCalculated()
        {
            _sut.CalculateEarnings();
            _sut.EarningsProfile.OnProgramTotal.Should().Be(_sut.AgreedPrice * .8m);
        }

        [Test]
        public void ThenTheCompletionAmountIsCalculated()
        {
            _sut.CalculateEarnings();
            _sut.EarningsProfile.CompletionPayment.Should().Be(_sut.AgreedPrice * .2m);
        }

        [Test]
        public void ThenTheInstalmentsAreGeneratedWithAmountsTo5dp()
        {
            _sut.CalculateEarnings();

            _sut.EarningsProfile.Instalments.Count.Should().Be(12);
            _sut.EarningsProfile.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(decimal.Round(_sut.EarningsProfile.OnProgramTotal / 12m, 5)));
        }

        [Test]
        public void ThenEarningsCalculatedEventIsCreated()
        {
            _sut.CalculateEarnings();

            var events = _sut.FlushEvents();
            events.Should().ContainSingle(x => x.GetType() == typeof(EarningsCalculatedEvent));
        }
        
        [Test]
        public void ThenTheEarningsProfileIdIsGenerated()
        {
            _sut.CalculateEarnings();
            _sut.EarningsProfile.EarningsProfileId.Should().NotBeEmpty();
        }
    }
}