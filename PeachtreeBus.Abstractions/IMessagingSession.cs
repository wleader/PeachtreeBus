using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus;

public interface IMessagingSession
{
    Task Publish(Topic topic, ISubscribedMessage message);
    Task Publish(Topic topic, ISubscribedMessage message, DateTime? notBefore = null, int priority = 0, UserHeaders? userHeaders = null, bool newConveration = false);
    Task Send(IQueueMessage message, QueueName queueName);
    Task Send(IQueueMessage message, QueueName queueName, DateTime? notBefore = null, int priority = 0, UserHeaders? userHeaders = null, bool newConveration = false);
    Task SendLocal(IQueueMessage message);
}