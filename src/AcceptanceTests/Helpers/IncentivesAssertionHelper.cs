using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public static class IncentivesAssertionHelper
{
    public static void AssertIncentivePayment(string type, bool second, bool expectedPayment, ApprenticeshipStartDateChangedEvent startDateChangedEvent, ApprenticeshipModel apprenticeshipModel)
    {
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var expectedPeriod = second
            ? startDateChangedEvent.StartDate.AddDays(364).ToAcademicYearAndPeriod()
            : startDateChangedEvent.StartDate.AddDays(89).ToAcademicYearAndPeriod();

        if (expectedPayment)
            currentEpisode.EarningsProfile.AdditionalPayments.Should().Contain(x =>
                x.AcademicYear == expectedPeriod.AcademicYear
                && x.DeliveryPeriod == expectedPeriod.Period
                && x.AdditionalPaymentType == type
                && x.Amount == 500);
        else
            currentEpisode.EarningsProfile.AdditionalPayments.Should().NotContain(x =>
                x.AcademicYear == expectedPeriod.AcademicYear
                && x.DeliveryPeriod == expectedPeriod.Period
                && x.AdditionalPaymentType == type
                && x.Amount == 500);
    }
}