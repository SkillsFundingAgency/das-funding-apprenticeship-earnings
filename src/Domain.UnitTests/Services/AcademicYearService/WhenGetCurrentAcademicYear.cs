using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Services.AcademicYearService
{
    [TestFixture]
    public class WhenGetCurrentAcademicYear
    {
        private Mock<IDateService> _dateService;
        private Domain.Services.AcademicYearService _sut;

        [SetUp]
        public void SetUp()
        {
            _dateService = new Mock<IDateService>();
            _sut = new Domain.Services.AcademicYearService(_dateService.Object);
        }

        [Test]
        public void WhenBeforeAugustThenAcademicYearIsPreviousYearAndCurrentYear()
        {
            _dateService.Setup(x => x.Today).Returns(new DateTime(2022, 07, 31));

            var result = _sut.CurrentAcademicYear;

            result.Should().Be(2122);
        }

        [Test]
        public void WhenInAugustThenAcademicYearIsCurrentYearAndNextYear()
        {
            _dateService.Setup(x => x.Today).Returns(new DateTime(2022, 08, 01));

            var result = _sut.CurrentAcademicYear;

            result.Should().Be(2223);
        }

        [Test]
        public void WhenAfterAugustThenAcademicYearIsCurrentYearAndNextYear()
        {
            _dateService.Setup(x => x.Today).Returns(new DateTime(2022, 09, 01));

            var result = _sut.CurrentAcademicYear;

            result.Should().Be(2223);
        }
    }
}
