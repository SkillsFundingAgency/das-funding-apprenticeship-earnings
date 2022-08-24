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
        public void ThenTheOnProgramPriceIs80PercentOfTheAgreedPrice()
        {
            var fundingBandMaximum = _fixture.Create<decimal>();
            var agreedPrice = fundingBandMaximum;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(),_fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.OnProgramTotalAmount.Should().Be(agreedPrice * 0.8m);
        }

        [Test]
        public void ThenTheCompletionPaymentIs20PercentOfTheAgreedPrice()
        {
            var fundingBandMaximum = _fixture.Create<decimal>();
            var agreedPrice = fundingBandMaximum;
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.CompletionPayment.Should().Be(agreedPrice * 0.2m);
        }

        [Test]
        public void ThenTheOnProgramPriceIs80PercentOfTheCappedAgreedPriceWhereAgreedPriceIsAboveFundingBandMaximum()
        {
            var fundingBandMaximum = _fixture.Create<decimal>();
            var agreedPrice = fundingBandMaximum + _fixture.Create<decimal>();
            
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.OnProgramTotalAmount.Should().Be(fundingBandMaximum * 0.8m);
        }

        [Test]
        public void ThenTheCompletionPaymentIs20PercentOfTheCappedAgreedPriceWhereAgreedPriceIsAboveFundingBandMaximum()
        {
            var fundingBandMaximum = _fixture.Create<decimal>();
            var agreedPrice = fundingBandMaximum + _fixture.Create<decimal>();
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>(), fundingBandMaximum);

            apprenticeshipFunding.CompletionPayment.Should().Be(fundingBandMaximum * 0.2m);
        }
    }
}
