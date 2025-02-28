using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using System;

[assembly: NServiceBusTriggerFunction("SFA.DAS.Funding.ApprenticeshipEarnings", Connection = "AzureWebJobsServiceBus", TriggerFunctionName = "NServiceBusTriggerFunction")]

var host = new HostBuilder();

var startup = new Startup();
startup.Configure(host);

var app = host.Build();
app.Run();