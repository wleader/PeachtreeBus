using System;

namespace PeachtreeBus.Example.Messages
{
    public class SampleSagaStart : IMessage
    {
        public Guid AppId { get; set; }
    }

    public class SampleSagaComplete : IMessage
    {
        public Guid AppId { get; set; }
    }

    public class SampleDistributedTaskRequest : IMessage
    {
        public Guid AppId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; }
    }

    public class SampleDistributedTaskResponse : IMessage
    {
        public Guid AppId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; }
        public int Result { get; set; }
    }
}
