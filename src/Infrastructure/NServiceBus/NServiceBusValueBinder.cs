using Microsoft.Azure.WebJobs.Host.Bindings;
using System.Reflection;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.NServiceBus
{
    public class NServiceBusMessageValueBinder : IValueBinder
    {
        private object _value;

        public NServiceBusMessageValueBinder(ParameterInfo parameter, object value)
        {
            _value = value;
            Type = parameter.ParameterType;
        }

        public Task<object> GetValueAsync()
        {
            return Task.FromResult(_value);
        }

        public string ToInvokeString()
        {
            return _value?.ToString();
        }

        public Type Type { get; }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            _value = value;

            return Task.CompletedTask;
        }
    }
}
