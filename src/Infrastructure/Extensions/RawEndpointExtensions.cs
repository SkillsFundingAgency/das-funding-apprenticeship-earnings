using NServiceBus.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.Infrastructure.Extensions
{
    public static class RawEndpointConfigurationExtension
    {
        public static void UseLicense(this RawEndpointConfiguration config, string licenseText)
        {
            if (string.IsNullOrWhiteSpace(licenseText))
            {
                throw new ArgumentException("NServiceBus license text must not be null or white space", nameof(licenseText));
            }

            config.Settings.Set("LicenseText", licenseText);
        }
    }
}
