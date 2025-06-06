namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;

public static class ContextKeys
{
    public const string ExpectedDeliveryPeriodCount = "expectedDeliveryPeriodCount";
    public const string ExpectedDeliveryPeriodLearningAmount = "expectedDeliveryPeriodLearningAmount";
}

public static class ApprenticeshipCreatedEventDefaults
{
    public static DateTime StartDate = new DateTime(2019, 01, 01);
    public static DateTime DateOfBirth = new DateTime(2000, 1, 1);
    public static DateTime EndDate = new DateTime(2021, 1, 1);
    public static int AgeAtStartOfApprenticeship = 21;
    public static long ApprovalsApprenticeshipId = 120;
    public static Apprenticeships.Enums.FundingPlatform FundingPlatform = Apprenticeships.Enums.FundingPlatform.DAS;
    public static decimal TotalPrice = 15000m;
    public static int FundingBandMaximum = 15000;
}
