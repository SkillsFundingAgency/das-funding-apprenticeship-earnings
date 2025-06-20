using NServiceBus.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.LogCorrelation
{
    public static class CorrelationContext
    {
        private static readonly AsyncLocal<string> _correlationId = new();

        public static string CorrelationId
        {
            get => _correlationId.Value;
            set => _correlationId.Value = value;
        }
    }

    public class IncomingCorrelationIdBehavior : Behavior<IIncomingLogicalMessageContext>
    {
        public override Task Invoke(IIncomingLogicalMessageContext context, Func<Task> next)
        {
            if (context.Headers.TryGetValue("x-correlation-id", out var correlationId))
            {
                CorrelationContext.CorrelationId = correlationId;
            }
            else
            {
                CorrelationContext.CorrelationId = Guid.NewGuid().ToString();
            }

            return next();
        }
    }

    public class OutgoingCorrelationIdBehavior : Behavior<IOutgoingLogicalMessageContext>
    {
        public override Task Invoke(IOutgoingLogicalMessageContext context, Func<Task> next)
        {
            var correlationId = CorrelationContext.CorrelationId ?? Guid.NewGuid().ToString();
            context.Headers["x-correlation-id"] = correlationId;

            return next();
        }
    }
}
