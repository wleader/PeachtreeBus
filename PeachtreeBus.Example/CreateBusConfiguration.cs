using PeachtreeBus.Example.Subsciptions;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Example;

public static class CreateBusConfiguration
{
    public static IBusConfiguration Create(string connectionString)
    {
        return new BusConfiguration()
        {
            // requried. This must always be configured.
            Schema = new("PeachtreeBus"),
            // required. What SQL Server database to use.
            ConnectionString = connectionString,

            // if configured this process should recieve and process messages from this queue.
            QueueConfiguration = new()
            {
                QueueName = new("SampleQueue"),

                // Determines if the default IHandleFailedQueueMessages will be registerd.
                // The default handler does nothing.
                // If false, you must register your own implementation of IHandleFailedQueueMessages with the container.
                UseDefaultFailedHandler = true,

                // Determines if the default IQueueRetryStrategy will be registered.
                // The Default strategy retries up to 5 times, waiting 5 seconds longer after each failure.
                // if false you must register your own implementation of IQueueRetryStrategy
                UseDefaultRetryStrategy = true,

                // determines which messages to automatically cleaned up.
                CleanFailed = true,
                CleanCompleted = true,
                // determines how old message have to be to be cleaned.
                CleanFailedAge = TimeSpan.FromDays(7),
                CleanCompleteAge = TimeSpan.FromDays(1),
                // how often to perform the cleanup.
                CleanInterval = TimeSpan.FromMinutes(5),

            },

            // If configured this causes the process to search for and process subscribed messages.
            SubscriptionConfiguration = new()
            {
                // In a real application, each instance of the process would have a different ID.
                // this can be random, or managed as needed. 
                SubscriberId = new SubscriberId(Guid.Parse("E00E876C-A9F4-46C4-B0E7-2B27C525FA98")),

                // Causes the process to put into the subscriptions table what categories of
                // published messages it wants to recieve.
                // If Empty, then the process will subscribe to nothing, and no messages will
                // be published to the subscriber.
                Topics = [Topics.Announcements],

                // When adding or updating the subscriptions table, this determines how long the subscription
                // is considered valid for. If the subscription is updated, it will be removed after this amount of time.
                Lifespan = TimeSpan.FromDays(1),

                // Determines if the default IHandleFailedSubscribedMessages will be registerd.
                // The default handler does nothing.
                // If false, you must register your own implementation of IHandleFailedSubscribedMessages with the container.
                UseDefaultFailedHandler = true,

                // Determines if the default ISubscribedRetryStrategy will be registered.
                // The Default strategy retries up to 5 times, waiting 5 seconds longer after each failure.
                // if false you must register your own implementation of ISubscribedRetryStrategy
                UseDefaultRetryStrategy = true,

                // determines which messages to automatically cleaned up.
                CleanFailed = true,
                CleanCompleted = true,
                // determines how old message have to be to be cleaned.
                CleanFailedAge = TimeSpan.FromDays(7),
                CleanCompleteAge = TimeSpan.FromDays(1),
                // how often to perform the cleanup.
                CleanInterval = TimeSpan.FromMinutes(5),
            },

            PublishConfiguration = new()
            {
                // When publishing a message to subscribers, this determins how long the message
                // can stay in the pending messages before it is considered abandoned.
                Lifespan = TimeSpan.FromDays(1),
            },
        };
    }
}
