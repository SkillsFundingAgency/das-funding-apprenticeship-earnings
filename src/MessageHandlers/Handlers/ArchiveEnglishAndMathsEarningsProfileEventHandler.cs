using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command;
using SFA.DAS.Funding.ApprenticeshipEarnings.Command.ArchiveEnglishAndMathsEarningsProfileCommand;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

public class ArchiveEnglishAndMathsEarningsProfileEventHandler(
    ICommandHandler<ArchiveEnglishAndMathsEarningsProfileCommand> englishAndMathsArchiveCommandHandler,
    ILogger<ArchiveEnglishAndMathsEarningsProfileEventHandler> logger)
    : IHandleMessages<EarningsProfileForEnglishAndMathsUpdatedEvent>
{
    public async Task Handle(EarningsProfileForEnglishAndMathsUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{Handler} processing...", nameof(EarningsProfileForEnglishAndMathsUpdatedEvent));

        logger.LogInformation("EarningsProfileId: {Key} EnglishAndMathsKey: {EnglishAndMathsKey} Received {EventName}",
            message.EarningsProfileId,
            message.EnglishAndMathsKey,
            nameof(EarningsProfileForEnglishAndMathsUpdatedEvent));

        await englishAndMathsArchiveCommandHandler.Handle(new ArchiveEnglishAndMathsEarningsProfileCommand(message), context.CancellationToken);
    }
}
