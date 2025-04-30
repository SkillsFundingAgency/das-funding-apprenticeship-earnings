namespace SFA.DAS.Funding.ApprenticeshipEarnings.Domain;

public static class Constants
{
    public const decimal GovernmentContribution = 0.95m;
    public const decimal EmployerContribution = 0.05m;
    public const int QualifyingPeriod = 42;
}

public static class InstalmentTypes
{
    public const string OnProgramme = "OnProgramme";
    public const string ProviderIncentive = "ProviderIncentive";
    public const string EmployerIncentive = "EmployerIncentive";
    public const string LearningSupport = "LearningSupport";
}

public static class AdditionalPaymentAmounts
{
    public const decimal Incentive = 500;
    public const decimal LearningSupport = 150;
}