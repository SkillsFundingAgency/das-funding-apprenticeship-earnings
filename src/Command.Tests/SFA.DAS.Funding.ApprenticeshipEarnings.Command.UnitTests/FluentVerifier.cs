using FluentAssertions.Execution;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.UnitTests
{
    public static class FluentVerifier
    {
        public static bool VerifyFluentAssertion(Action assertion)
        {
            using var assertionScope = new AssertionScope();
            assertion();

            return !assertionScope.Discard().Any();
        }
    }
}
