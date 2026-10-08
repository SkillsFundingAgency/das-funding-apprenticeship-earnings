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
    : IHandleMessages<EnglishAndMathsEarningsProfileUpdatedEvent>
{
    public async Task Handle(EnglishAndMathsEarningsProfileUpdatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{functionName} processing...", nameof(EnglishAndMathsEarningsProfileUpdatedEvent));

        logger.LogInformation("EarningsProfileId: {Key} EnglishAndMathsKey: {EnglishAndMathsKey} Received {EventName}",
            message.EarningsProfileId,
            message.EnglishAndMathsKey,
            nameof(EnglishAndMathsEarningsProfileUpdatedEvent));

        await englishAndMathsArchiveCommandHandler.Handle(new ArchiveEnglishAndMathsEarningsProfileCommand(message), context.CancellationToken);
    }
}
