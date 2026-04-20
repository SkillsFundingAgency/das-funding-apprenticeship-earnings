using Azure.Messaging.ServiceBus.Administration;

namespace SFA.DAS.ServiceBus.Implementation;

public interface IServiceBusInfrastructureBuilder
{
    Task EnsureAsync(CancellationToken ct = default);
}

internal class ServiceBusInfrastructureBuilder : IServiceBusInfrastructureBuilder
{
    private readonly ServiceBusAdministrationClient _admin;
    private readonly ServiceBusConfig _config;
    private readonly IMessageHandlerRegistry _registry;

    public ServiceBusInfrastructureBuilder(
        ServiceBusAdministrationClient admin,
        ServiceBusConfig config,
        IMessageHandlerRegistry registry)
    {
        _admin = admin;
        _config = config;
        _registry = registry;
    }

    public async Task EnsureAsync(CancellationToken cancellationToken = default)
    {
        // 1. Topic
        if (!await _admin.TopicExistsAsync(_config.TopicName, cancellationToken))
        {
            await _admin.CreateTopicAsync(_config.TopicName, cancellationToken);
        }

        // 2. Queue
        if (!await _admin.QueueExistsAsync(_config.QueueName, cancellationToken))
        {
            await _admin.CreateQueueAsync(_config.QueueName, cancellationToken);
        }

        // 3. Subscription
        var subscription = await ResolveSubscription(cancellationToken);

        // 4. Filters
        await EnsureFilters(subscription, cancellationToken);
    }

    private async Task<SubscriptionProperties> ResolveSubscription(CancellationToken cancellationToken)  
    {
        var subscriptions = _admin.GetSubscriptionsAsync(_config.TopicName);
        SubscriptionProperties? existingSub = null;

        await foreach (var sub in subscriptions)
        {
            if (sub.ForwardTo.Contains(_config.QueueName))
            {
                existingSub = sub;
                break;
            }
        }

        if (existingSub == null)
        {
            var subName = _config.QueueName ?? Guid.NewGuid().ToString();

            var options = new CreateSubscriptionOptions(
                _config.TopicName,
                subName)
            {
                ForwardTo = _config.QueueName
            };

            await _admin.CreateSubscriptionAsync(options, cancellationToken);

            // remove default rule (critical!)
            await _admin.DeleteRuleAsync(
                _config.TopicName,
                subName,
                RuleProperties.DefaultRuleName);

            existingSub = await _admin.GetSubscriptionAsync(_config.TopicName, subName, cancellationToken);
        }

        return existingSub;
    }

    private async Task EnsureFilters(SubscriptionProperties subscription, CancellationToken cancellationToken)
    {
        var rules = _admin.GetRulesAsync(_config.TopicName, subscription.SubscriptionName, cancellationToken);

        var existingRules = new List<SqlRuleFilter>();

        await foreach (var rule in rules)
        {
            if (rule.Filter is SqlRuleFilter sqlRule) 
            { 
                existingRules.Add(sqlRule); 
            }
        }

        foreach (var handler in _registry.GetAll())
        {
            var filter = new SqlRuleFilter(
                $"[NServiceBus.EnclosedMessageTypes] LIKE '%{handler.HandledEventType.Name}%'");

            var ruleName = AzureRuleNameShortener.Shorten(handler.HandledEventType);

            if (existingRules.Any(x=>x.SqlExpression == filter.SqlExpression))
                continue;

            await _admin.CreateRuleAsync(
                _config.TopicName,
                subscription.SubscriptionName,
                new CreateRuleOptions(ruleName, filter),
                cancellationToken);
        }
    }
}