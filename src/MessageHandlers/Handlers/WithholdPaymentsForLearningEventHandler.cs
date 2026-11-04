using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Configuration;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers.Handlers;

/// <summary>
/// Forwards on the WithholdPaymentsForLearningEvent to the Payments service.
/// This is the same Publish-and-Send pattern as elsewhere, allowing automation tests to subscribe.
/// </summary>
public class WithholdPaymentsForLearningEventHandler(
    PaymentsConfiguration paymentsConfiguration,
    ILogger<WithholdPaymentsForLearningEventHandler> logger)
    : IHandleMessages<WithholdPaymentsForLearningEvent>
{
    public async Task Handle(WithholdPaymentsForLearningEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("{HandlerName} sending withhold to Payments for LearningKey: {LearningKey}", nameof(WithholdPaymentsForLearningEventHandler), message.LearningKey);

        var options = new SendOptions();
        options.DoNotEnforceBestPractices(); //This is an event, so overlook best practices in order to Send() it
        options.SetDestination(paymentsConfiguration.PaymentsEndpoint);

        await context.Send(message, options);
    }
}
