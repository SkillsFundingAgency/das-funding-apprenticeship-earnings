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

            Assert.AreEqual(apprenticeshipEntityModel.ActualStartDate, apprenticeship.ActualStartDate);
            Assert.AreEqual(apprenticeshipEntityModel.AgreedPrice, apprenticeship.AgreedPrice);
            Assert.AreEqual(apprenticeshipEntityModel.EmployerAccountId, apprenticeship.EmployerAccountId);
            Assert.AreEqual(apprenticeshipEntityModel.FundingEmployerAccountId, apprenticeship.FundingEmployerAccountId); 
            Assert.AreEqual(apprenticeshipEntityModel.LegalEntityName, apprenticeship.LegalEntityName);
            Assert.AreEqual(apprenticeshipEntityModel.PlannedEndDate, apprenticeship.PlannedEndDate);
            Assert.AreEqual(apprenticeshipEntityModel.TrainingCode, apprenticeship.TrainingCode);
            Assert.AreEqual(apprenticeshipEntityModel.UKPRN, apprenticeship.UKPRN);
            Assert.AreEqual(apprenticeshipEntityModel.ApprenticeshipKey, apprenticeship.ApprenticeshipKey);
            Assert.AreEqual(apprenticeshipEntityModel.ApprovalsApprenticeshipId, apprenticeship.ApprovalsApprenticeshipId);
            Assert.AreEqual(apprenticeshipEntityModel.Uln, apprenticeship.Uln);

        }
    }
}
