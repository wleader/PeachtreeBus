using PeachtreeBus.Exceptions;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus;

public class MessagingSession(
    IQueueWriter queueWriter,
    ISubscribedPublisher publisher,
    IBusConfiguration configuration)
    : IMessagingSession
{
    public Task Send(IQueueMessage message, QueueName queueName,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null,
        bool newConveration = false) =>
        queueWriter.WriteMessage(queueName, message, notBefore, priority, userHeaders, newConveration);

    public Task Send(IQueueMessage message, QueueName queueName) =>
        queueWriter.WriteMessage(queueName, message, null, 0, null, false);

    public async Task SendLocal(IQueueMessage message)
    {
        var queueConfiguration = configuration.QueueConfiguration
            ?? throw new ConfigurationException(
                $"SendLocal requires that the IBusConfiguration.QueueConfiguration is not null.");
        await Send(message, queueConfiguration.QueueName);
    }

    public Task Publish(Topic topic, ISubscribedMessage message,
        DateTime? notBefore = null,
        int priority = 0,
        UserHeaders? userHeaders = null,
        bool newConveration = false) =>
        publisher.Publish(topic, message, notBefore, priority, userHeaders, newConveration);

    public Task Publish(Topic topic, ISubscribedMessage message) =>
        publisher.Publish(topic, message, null, 0, null, false);

}
