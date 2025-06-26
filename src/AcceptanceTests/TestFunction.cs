using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using NServiceBus.Testing;
using SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests.Helpers;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using SFA.DAS.Testing.AzureStorageEmulator;

namespace SFA.DAS.Funding.ApprenticeshipEarnings.AcceptanceTests;

public class TestFunction : IDisposable
{
    private readonly TestContext _testContext;
    private readonly TestServer _testServer;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private bool _isDisposed;

    public string HubName { get; }

    public TestFunction(TestContext testContext, string hubName)
    {
        var _ = new Startup();// This forces the AzureFunction assembly to load
        _queueTriggeredFunctions = MessageHandlerHelper.GetMessageHandlers();


        AzureStorageEmulatorManager.StartStorageEmulator();

        HubName = hubName;
        _testContext = testContext;
        _testContext.MessageSession.RegisterSubscriber<object>(PublishEvent);

        _testServer = new TestServer(new WebHostBuilder()
            .UseEnvironment(Environments.Development)
            .UseStartup<TestStartup>((_) => new TestStartup(testContext, _queueTriggeredFunctions, _testContext.MessageSession)));

    }

    public async Task PublishEvent<T>(T eventObject)
    {
        var eventType = eventObject.GetType();

        var function = _queueTriggeredFunctions.FirstOrDefault(x => x.HandledEventType == eventType);
        if (function == null)
            return;

        var handler = _testServer.Services.GetService(function.HandlerType);
        var context = new TestableMessageHandlerContext
        {
            CancellationToken = new CancellationToken()
        };

        // Call the correct generic method dynamically
        var handleMethod = function.HandlerType.GetMethod("Handle", new[] { eventType, typeof(IMessageHandlerContext) });
        if (handleMethod == null)
            throw new InvalidOperationException("Handle method not found on handler");

        var task = (Task)handleMethod.Invoke(handler, new object[] { eventObject, context });
        await task;
    }


    public async Task DisposeAsync()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // no components to dispose
        }

        _isDisposed = true;
    }

}