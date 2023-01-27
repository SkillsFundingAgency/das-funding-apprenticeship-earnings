using FluentAssertions;
using NUnit.Framework;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain.UnitTests
{
    [TestFixture]
    public class CoInvestmentTests
    {
        [Test]
        public void WhenCoInvestmentCalculatedEmployerContributionIs95Percent()
        {
            var totalAmount = 2000;
            var coinvestment = CoInvestment.Calculate(totalAmount);

            coinvestment.EmployerContribution.Should().Be(100);
        }

        [Test]
        public void WhenCoInvestmentCalculatedGovernmentContributionIs5Percent()
        {
            var totalAmount = 2000;
            var coinvestment = CoInvestment.Calculate(totalAmount);

            coinvestment.GovernmentContribution.Should().Be(1900);
        }
    }
}
