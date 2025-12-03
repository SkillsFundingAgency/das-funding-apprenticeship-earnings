using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.TestHelpers;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Factories.ApprenticeshipFactory
{
    [TestFixture]
    public class WhenDeterminingFundingLineType
    {
        private Fixture _fixture;
        private Domain.Factories.ApprenticeshipFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _factory = new Domain.Factories.ApprenticeshipFactory();
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeUnder19()
        {
            var apprenticeship = _fixture.CreateApprenticeship(age: 18);
            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("16-18 Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIs19()
        {
            var apprenticeship = _fixture.CreateApprenticeship(age: 19);

            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIsOver19()
        {
            var apprenticeship = _fixture.CreateApprenticeship(age: 20);

            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }
    }
}
