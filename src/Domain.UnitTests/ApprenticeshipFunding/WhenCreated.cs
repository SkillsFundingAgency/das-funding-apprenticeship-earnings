using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding
{
    [TestFixture]
    public class WhenCreated
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void WhenAgreedPriceLessThenFundingBandMaximumThenTheTotalOnProgrammeAmountIs80PercentOfTheAdjustedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var fundingBandMaximum = agreedPrice + 1;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(),_fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.AdjustedPrice.Should().Be(agreedPrice);
            apprenticeshipFunding.OnProgramTotal.Should().Be(agreedPrice * 0.8m);
        }

        [Test]
        public void WhenAgreedPriceLessThenFundingBandMaximumThenTheCompletionPaymentIs20PercentOfTheAdjustedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>(); 
            var fundingBandMaximum = agreedPrice + 1;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.AdjustedPrice.Should().Be(agreedPrice);
            apprenticeshipFunding.CompletionPayment.Should().Be(agreedPrice * 0.2m);
        }

        [Test]
        public void WhenAgreedPriceMoreThenFundingBandMaximumThenTheTotalOnProgrammeAmountIs80PercentOfTheAdjustedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var fundingBandMaximum = agreedPrice - 1000;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.AdjustedPrice.Should().Be(fundingBandMaximum);
            apprenticeshipFunding.OnProgramTotal.Should().Be(fundingBandMaximum * 0.8m);
        }

        [Test]
        public void WhenAgreedPriceMoreThenFundingBandMaximumThenTheCompletionPaymentIs20PercentOfTheAdjustedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var fundingBandMaximum = agreedPrice - 1000;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.AdjustedPrice.Should().Be(fundingBandMaximum);
            apprenticeshipFunding.CompletionPayment.Should().Be(fundingBandMaximum * 0.2m);
        }
    }
}
