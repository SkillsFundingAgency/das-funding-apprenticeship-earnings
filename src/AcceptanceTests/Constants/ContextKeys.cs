using SFA.DAS.Apprenticeships.Enums;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Constants;

public static class ContextKeys
{
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
    public static long EmployerAccountId = 12345678;

    public static int ExpectedDeliveryPeriodCount = 24;
    public static int ExpectedDeliveryPeriodLearningAmount = 500;
}

public static class ApprenticeshipPriceChangedEventDefaults
{
    public static DateTime EffectiveFromDate = new DateTime(2020, 02, 01);
    public static ApprovedBy ApprovedBy = ApprovedBy.Employer;
    public static DateTime ApprovedDate = new DateTime(2020, 1, 1);
    public static DateTime StartDate = new DateTime(2019, 9, 1);
}
