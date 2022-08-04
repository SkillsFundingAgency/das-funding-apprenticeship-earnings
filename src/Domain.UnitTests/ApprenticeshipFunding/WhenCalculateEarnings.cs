using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Events;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.ApprenticeshipFunding
{
    [TestFixture]
    public class WhenCalculateEarnings
    {
        private Fixture _fixture;
        private Apprenticeship.Apprenticeship _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new Apprenticeship.Apprenticeship(
                Guid.NewGuid(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                new DateTime(2021, 1, 15),
                new DateTime(2022, 1, 15),
                _fixture.Create<decimal>(),
                _fixture.Create<string>(),
                null,
                _fixture.Create<FundingType>());
        }

        [Test]
        public void ThenTheAdjustedPriceIsCalculated()
        {
            _sut.CalculateEarnings();
            _sut.EarningsProfile.AdjustedPrice.Should().Be(_sut.AgreedPrice * .8m);
        }

        [Test]
        public void ThenTheCompletionAmountIsCalculated()
        {
            _sut.CalculateEarnings();
            _sut.EarningsProfile.CompletionPayment.Should().Be(_sut.AgreedPrice * .2m);
        }

        [Test]
        public void ThenTheInstalmentsAreGenerated()
        {
            _sut.CalculateEarnings();

            _sut.EarningsProfile.Instalments.Count.Should().Be(13);
            _sut.EarningsProfile.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(_sut.EarningsProfile.AdjustedPrice / 13m));
        }
    }
}
