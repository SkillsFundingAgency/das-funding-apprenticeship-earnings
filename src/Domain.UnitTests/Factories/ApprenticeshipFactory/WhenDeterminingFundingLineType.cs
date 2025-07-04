using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;

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
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipModel>().Create();
            apprenticeshipEntityModel.Episodes = new List<EpisodeModel>
            {
                _fixture.Build<EpisodeModel>().With(x => x.AgeAtStartOfApprenticeship, 18).Create()
            };
            var apprenticeship = _factory.GetExisting(apprenticeshipEntityModel);

            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("16-18 Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIs19()
        {
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipModel>().Create();
            apprenticeshipEntityModel.Episodes = new List<EpisodeModel>
            {
                _fixture.Build<EpisodeModel>().With(x => x.AgeAtStartOfApprenticeship, 19).Create()
            };

            var apprenticeship = _factory.GetExisting(apprenticeshipEntityModel);

            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }

        [Test]
        public void ThenTheFundingLineTypeIsCorrectWhenApprenticeIsOver19()
        {
            var apprenticeshipEntityModel = _fixture.Build<ApprenticeshipModel>().Create();
            apprenticeshipEntityModel.Episodes = new List<EpisodeModel>
            {
                _fixture.Build<EpisodeModel>().With(x => x.AgeAtStartOfApprenticeship, 20).Create()
            };
            var apprenticeship = _factory.GetExisting(apprenticeshipEntityModel);

            apprenticeship.ApprenticeshipEpisodes.Single().FundingLineType.Should().Be("19+ Apprenticeship (Employer on App Service)");
        }
    }
}
