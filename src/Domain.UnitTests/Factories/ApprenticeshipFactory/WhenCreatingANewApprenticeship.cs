using AutoFixture;
using NUnit.Framework;
using SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests.Factories.ApprenticeshipFactory
{
    [TestFixture]
    public class WhenCreatingANewApprenticeship
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
        public void ThenTheApprenticeshipIsCreated()
        {
            var apprenticeshipEntityModel = _fixture.Create<ApprenticeshipEntityModel>();

            var apprenticeship = _factory.CreateNew(apprenticeshipEntityModel);

            Assert.That(apprenticeshipEntityModel.ActualStartDate, Is.EqualTo(apprenticeship.ActualStartDate));
            Assert.That(apprenticeshipEntityModel.AgreedPrice, Is.EqualTo(apprenticeship.AgreedPrice));
            Assert.That(apprenticeshipEntityModel.EmployerAccountId, Is.EqualTo(apprenticeship.EmployerAccountId));
            Assert.That(apprenticeshipEntityModel.FundingEmployerAccountId, Is.EqualTo(apprenticeship.FundingEmployerAccountId)); 
            Assert.That(apprenticeshipEntityModel.LegalEntityName, Is.EqualTo(apprenticeship.LegalEntityName));
            Assert.That(apprenticeshipEntityModel.PlannedEndDate, Is.EqualTo(apprenticeship.PlannedEndDate));
            Assert.That(apprenticeshipEntityModel.TrainingCode, Is.EqualTo(apprenticeship.TrainingCode));
            Assert.That(apprenticeshipEntityModel.UKPRN, Is.EqualTo(apprenticeship.UKPRN));
            Assert.That(apprenticeshipEntityModel.ApprenticeshipKey, Is.EqualTo(apprenticeship.ApprenticeshipKey));
            Assert.That(apprenticeshipEntityModel.ApprovalsApprenticeshipId, Is.EqualTo(apprenticeship.ApprovalsApprenticeshipId));
            Assert.That(apprenticeshipEntityModel.Uln, Is.EqualTo(apprenticeship.Uln));
            Assert.That(apprenticeshipEntityModel.FundingBandMaximum, Is.EqualTo(apprenticeship.FundingBandMaximum));
            Assert.That(apprenticeshipEntityModel.AgeAtStartOfApprenticeship, Is.EqualTo(apprenticeship.AgeAtStartOfApprenticeship));
        }
    }
}
