using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

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
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipEntityModel>().With(x => x.AgeAtStartOfApprenticeship, 18).Create();

            var apprenticeship = _factory.CreateNew(apprenticeshipEntityModel);

            apprenticeship.FundingLineType.Should().Be("16-18 Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIs19()
        {
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipEntityModel>().With(x => x.AgeAtStartOfApprenticeship, 19).Create();

            var apprenticeship = _factory.CreateNew(apprenticeshipEntityModel);

            apprenticeship.FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIsOver19()
        {
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipEntityModel>().With(x => x.AgeAtStartOfApprenticeship, 20).Create();

            var apprenticeship = _factory.CreateNew(apprenticeshipEntityModel);

            apprenticeship.FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }
    }
}
