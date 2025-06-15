using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Example.Messages
{
    /// <summary>
    /// A message that causes the Example saga to be started.
    /// </summary>
    public class SampleSagaStart : IQueueMessage
    {
        public Guid AppId { get; set; }
    }

    /// <summary>
    /// A message that is sent when the example saga is completed.
    /// </summary>
    public class SampleSagaComplete : IQueueMessage
    {
        public Guid AppId { get; set; }
    }

    /// <summary>
    /// A Subscribed Message that the sample saga has been completed
    /// that subscribers may subscribe to.
    /// </summary>
    public class AnnounceSagaCompleted : ISubscribedMessage
    {
        public Guid AppId { get; set; }
    }

    /// <summary>
    /// A message where the sample saga has distributed work to
    /// be processed by another handler.
    /// </summary>
    public class SampleDistributedTaskRequest : IQueueMessage
    {
        public Guid AppId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; } = string.Empty;
    }

    /// <summary>
    /// The response to SampleDistributedTaskRequest
    /// </summary>
    public class SampleDistributedTaskResponse : IQueueMessage
    {
        public Guid AppId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; } = string.Empty;
        public int Result { get; set; }
    }
}
