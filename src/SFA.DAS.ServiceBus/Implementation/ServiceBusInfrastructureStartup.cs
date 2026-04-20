using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.ServiceBus.Implementation;

internal class ServiceBusStartupHostedService : IHostedService
{
    private readonly ITopologyInitializer _initializer;

    public ServiceBusStartupHostedService(ITopologyInitializer initializer)
    {
        _initializer = initializer;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.EnsureAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal class ServiceBusInfrastructureStartup
{
    private readonly ServiceBusAdministrationClient _admin;
    private readonly ServiceBusConfig _config;
    private readonly IMessageHandlerRegistry _registry;

    internal ServiceBusInfrastructureStartup(
        ServiceBusAdministrationClient admin,
        ServiceBusConfig config,
        IMessageHandlerRegistry registry)
    {
        _admin = admin;
        _config = config;
        _registry = registry;
    }

    internal async Task EnsureTopologyAsync(CancellationToken ct = default)
    {
        // 1. Topic
        if (!await _admin.TopicExistsAsync(_config.TopicName, ct))
        {
            await _admin.CreateTopicAsync(_config.TopicName, ct);
        }

        // 2. Queue
        if (!await _admin.QueueExistsAsync(_config.QueueName, ct))
        {
            await _admin.CreateQueueAsync(_config.QueueName, ct);
        }

        // 3. Subscription
        if (!await _admin.SubscriptionExistsAsync(_config.TopicName, _config.SubscriptionName, ct))
        {
            var options = new CreateSubscriptionOptions(
                _config.TopicName,
                _config.SubscriptionName)
            {
                ForwardTo = _config.QueueName // 🔥 auto-forward
            };

            await _admin.CreateSubscriptionAsync(options, ct);
        }

        // 4. Filters
        await EnsureFilters(ct);
    }

    private async Task EnsureFilters(CancellationToken ct)
    {
        var rules = _admin.GetRulesAsync(_config.TopicName, _config.SubscriptionName);

        var existingRules = new HashSet<string>();

        await foreach (var rule in rules)
        {
            existingRules.Add(rule.Name);
        }

        foreach (var handler in _registry.GetAll())
        {
            var ruleName = handler.HandledEventType.FullName;

            if (existingRules.Contains(ruleName))
                continue;

            var filter = new SqlRuleFilter(
                $"NServiceBus.EnclosedMessageTypes LIKE '%{handler.HandledEventType.FullName}%'");

            await _admin.CreateRuleAsync(
                _config.TopicName,
                _config.SubscriptionName,
                new CreateRuleOptions(ruleName, filter),
                ct);
        }
    }
}