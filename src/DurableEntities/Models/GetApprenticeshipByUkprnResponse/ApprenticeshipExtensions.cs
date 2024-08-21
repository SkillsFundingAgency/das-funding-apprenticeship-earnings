using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Apprenticeship;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.DurableEntities.Models.GetApprenticeshipByUkprnResponse;

internal static class ApprenticeshipExtensions
{
    /// <summary>
    /// The query endpoint to return apprenticeships by Ukprn expect the response in a format that does not match
    /// any of the internal apprenticeship models. This extension method is used to convert the internal apprenticeship
    /// </summary>
    internal static Apprenticeship ToApprenticeshipReponse(this Domain.Apprenticeship.Apprenticeship source, ISystemClockService systemClockService)
    {
        var currentEpisode = source.GetCurrentEpisode(systemClockService);
        return new Apprenticeship
        {
            Key = source.ApprenticeshipKey,
            Ukprn = currentEpisode.UKPRN,
            Episodes = source.ApprenticeshipEpisodes.Select(x => new Episode
            {
                Key = x.ApprenticeshipEpisodeKey,
                NumberOfInstalments = x.EarningsProfile.Instalments.Count,
                Instalments = x.EarningsProfile.Instalments.Select(i => new Instalment
                {
                    AcademicYear = i.AcademicYear,
                    DeliveryPeriod = i.DeliveryPeriod,
                    Amount = i.Amount
                }).ToList(),
                CompletionPayment = x.EarningsProfile.CompletionPayment,
                OnProgramTotal = x.EarningsProfile.OnProgramTotal
            }).ToList(),
            FundingLineType = currentEpisode.FundingType.ToString()
        };
    }
}
