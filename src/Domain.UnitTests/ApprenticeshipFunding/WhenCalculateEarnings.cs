﻿using System;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Apprenticeships.Types;

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
            var agreedPrice = _fixture.Create<decimal>();
            _sut = new Apprenticeship.Apprenticeship(
                Guid.NewGuid(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                _fixture.Create<long>(),
                _fixture.Create<long>(),
                _fixture.Create<string>(),
                new DateTime(2021, 1, 15),
                new DateTime(2022, 1, 15),
                agreedPrice,
                _fixture.Create<string>(),
                null,
                _fixture.Create<FundingType>(),
                agreedPrice + 1);
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
        public void ThenTheInstalmentsAreGenerated()
        {
            _sut.CalculateEarnings();

            _sut.EarningsProfile.Instalments.Count.Should().Be(13);
            _sut.EarningsProfile.Instalments.Should().AllSatisfy(x => x.Amount.Should().Be(_sut.EarningsProfile.OnProgramTotal / 13m));
        }
    }
}