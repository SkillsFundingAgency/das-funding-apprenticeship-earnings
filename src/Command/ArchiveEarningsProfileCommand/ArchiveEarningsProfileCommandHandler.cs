using Microsoft.Extensions.Logging;
using SFA.DAS.Funding.ApprenticeshipEarnings.DataAccess.Entities;
using SFA.DAS.Funding.ApprenticeshipEarnings.Domain.Repositories;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEarningsProfileCommand;


public class ArchiveEarningsProfileCommandHandler : ICommandHandler<ArchiveEarningsProfileCommand>
{
    private readonly IApprenticeshipRepository _apprenticeshipRepository;
    private readonly ILogger<ArchiveEarningsProfileCommandHandler> _logger;

    public ArchiveEarningsProfileCommandHandler(IApprenticeshipRepository apprenticeshipRepository, ILogger<ArchiveEarningsProfileCommandHandler> logger)
    {
        _apprenticeshipRepository = apprenticeshipRepository;
        _logger = logger;
    }

    public async Task Handle(ArchiveEarningsProfileCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("{handler} - Started", nameof(ArchiveEarningsProfileCommandHandler));

        var untrackedOriginalModel = CreateUntrackedEarningsProfileModel(command.ArchiveEarningsProfileEvent);

        _logger.LogInformation("{handler} - Creating untracked history model",nameof(ArchiveEarningsProfileCommandHandler));
        var untrackedHistoryModel = new EarningsProfileHistoryModel(untrackedOriginalModel, command.ArchiveEarningsProfileEvent.SupersededDate);
        
        _logger.LogInformation("{handler} - Adding untracked history model for EarningsProfileId: {EarningsProfileId} to repository",
            nameof(ArchiveEarningsProfileCommandHandler), untrackedHistoryModel.EarningsProfileId);
        await _apprenticeshipRepository.Add(untrackedHistoryModel);

        _logger.LogInformation("{handler} - Completed for {EarningsProfileId}",
            nameof(ArchiveEarningsProfileCommandHandler), untrackedOriginalModel.EarningsProfileId);
    }

    /// <summary>
    /// This creates a untracked earnings profile model which will match the data of the profile to be archived.
    /// </summary>
    private EarningsProfileModel CreateUntrackedEarningsProfileModel(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        _logger.LogInformation("{handler} - Creating untracked earnings profile model for EarningsProfileId: {EarningsProfileId}",
            nameof(ArchiveEarningsProfileCommandHandler), archiveEarningsProfileEvent.EarningsProfileId);
        return new EarningsProfileModel
        {
            EarningsProfileId = archiveEarningsProfileEvent.EarningsProfileId,
            EpisodeKey = archiveEarningsProfileEvent.EpisodeKey,
            Version = archiveEarningsProfileEvent.Version,
            OnProgramTotal = archiveEarningsProfileEvent.OnProgramTotal,
            CompletionPayment = archiveEarningsProfileEvent.CompletionPayment,
            Instalments = GetInstalments(archiveEarningsProfileEvent),
            AdditionalPayments = GetAdditionalPayments(archiveEarningsProfileEvent),
            MathsAndEnglishCourses = GetMathAndEnglish(archiveEarningsProfileEvent)
        };
    }

    private static List<InstalmentModel> GetInstalments(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        if (archiveEarningsProfileEvent.Instalments == null || !archiveEarningsProfileEvent.Instalments.Any())
        {
            return new List<InstalmentModel>();
        }

        return archiveEarningsProfileEvent.Instalments.Select(i => new InstalmentModel
        {
            Key = i.Key,
            EarningsProfileId = i.EarningsProfileId,
            EpisodePriceKey = i.EpisodePriceKey,
            AcademicYear = i.AcademicYear,
            DeliveryPeriod = i.DeliveryPeriod,
            Amount = i.Amount,
            Type = i.Type
        }).ToList();
    }

    private static List<AdditionalPaymentModel> GetAdditionalPayments(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        if (archiveEarningsProfileEvent.AdditionalPayments == null || !archiveEarningsProfileEvent.AdditionalPayments.Any())
        {
            return new List<AdditionalPaymentModel>();
        }

        return archiveEarningsProfileEvent.AdditionalPayments.Select(ap => new AdditionalPaymentModel
        {
            Key = ap.Key,
            EarningsProfileId = ap.EarningsProfileId,
            AcademicYear = ap.AcademicYear,
            DeliveryPeriod = ap.DeliveryPeriod,
            Amount = ap.Amount,
            AdditionalPaymentType = ap.AdditionalPaymentType,
            DueDate = ap.DueDate
        }).ToList();
    }

    private static List<MathsAndEnglishModel> GetMathAndEnglish(ArchiveEarningsProfileEvent archiveEarningsProfileEvent)
    {
        if (archiveEarningsProfileEvent.MathsAndEnglishCourses == null || !archiveEarningsProfileEvent.MathsAndEnglishCourses.Any())
        {
            return new List<MathsAndEnglishModel>();
        }

        return archiveEarningsProfileEvent.MathsAndEnglishCourses.Select(me => new MathsAndEnglishModel
        {
            Key = me.Key,
            EarningsProfileId = me.EarningsProfileId,
            Course = me.Course,
            StartDate = me.StartDate,
            EndDate = me.EndDate,
            Amount = me.Amount,
            Instalments = me.Instalments.Select(mi => new MathsAndEnglishInstalmentModel
            {
                Key = mi.Key,
                MathsAndEnglishKey = mi.MathsAndEnglishKey,
                AcademicYear = mi.AcademicYear,
                DeliveryPeriod = mi.DeliveryPeriod,
                Amount = mi.Amount
            }).ToList()
        }).ToList();
    }

}