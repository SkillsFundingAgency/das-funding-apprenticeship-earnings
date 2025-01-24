using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        private const int AzureServiceBusRuleNameMaxLength = 50;

        public static string FormatConnectionString(this string connectionString)
        {
            return connectionString.Replace("Endpoint=sb://", string.Empty).TrimEnd('/');
        }               
    }
}
