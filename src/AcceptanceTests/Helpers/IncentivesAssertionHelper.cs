using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Extensions;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.UpdateOnProgrammeCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.TestHelpers;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;

public static class IncentivesAssertionHelper
{
    public static void AssertIncentivePayment(string type, bool second, bool expectedPayment, UpdateOnProgrammeRequest updateOnProgrammeRequest, LearningModel apprenticeshipModel)
    {
        var currentEpisode = apprenticeshipModel!.GetCurrentEpisode(TestSystemClock.Instance());

        var startDate = updateOnProgrammeRequest.Prices.Min(p => p.StartDate);

        var expectedPeriod = second
            ? startDate.AddDays(364).ToAcademicYearAndPeriod()
            : startDate.AddDays(89).ToAcademicYearAndPeriod();

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