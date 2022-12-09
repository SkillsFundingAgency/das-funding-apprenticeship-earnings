using System;
using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Services.DateService
{
    [TestFixture]
    public class WhenGetToday
    {
        private Domain.Services.DateService _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new Domain.Services.DateService();
        }

        [Test]
        public void ThenTheCurrentDateIsReturned()
        {
            var expected = DateTime.Now.Date;
            var actual = _sut.Today;

            actual.Should().Be(expected);
        }
    }
}
