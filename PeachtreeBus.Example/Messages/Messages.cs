namespace PeachtreeBus.Example.Messages
{
    public class SampleSagaStart : IMessage
    {
        public int SagaId { get; set; }
    }

    public class SampleSagaComplete : IMessage
    {
        public int SagaId { get; set; }
    }

    public class SampleDistributedTaskRequest : IMessage
    {
        public int SagaId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; }
    }

    public class SampleDistributedTaskResponse : IMessage
    {
        public int SagaId { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public string Operation { get; set; }
        public int Result { get; set; }
    }
}
