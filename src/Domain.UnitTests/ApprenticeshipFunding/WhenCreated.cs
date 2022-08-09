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
        public void ThenTheAdjustedPriceIs80PercentOfTheAgreedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(),_fixture.Create<DateTime>());

            apprenticeshipFunding.AdjustedPrice.Should().Be(agreedPrice * 0.8m);
        }

        [Test]
        public void ThenTheCompletionPaymentIs20PercentOfTheAgreedPrice()
        {
            var agreedPrice = _fixture.Create<decimal>();
            var apprenticeshipFunding = new Domain.ApprenticeshipFunding.ApprenticeshipFunding(agreedPrice, _fixture.Create<DateTime>(), _fixture.Create<DateTime>());

            apprenticeshipFunding.CompletionPayment.Should().Be(agreedPrice * 0.2m);
        }
    }
}
