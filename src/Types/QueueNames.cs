namespace SFA.DAS.Funding.ApprenticeshipEarnings.Types
{
    public static class QueueNames
    {
        public const string ApprenticeshipCreated = "sfa-das-funding-apprenticeship-learners"; //todo we can change the value of this when we have sorted the queue name in the approvals event handlers app
        public const string EarningsGenerated = "SFA.DAS.Funding.ApprenticeshipEarnings";
    }
}
