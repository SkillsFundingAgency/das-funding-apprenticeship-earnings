using Microsoft.Extensions.Hosting;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;

var host = new HostBuilder();

var startup = new Startup();
startup.Configure(host);

var app = host.Build();
app.Run();