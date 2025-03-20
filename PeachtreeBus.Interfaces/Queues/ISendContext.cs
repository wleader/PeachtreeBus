namespace PeachtreeBus.Queues;

public interface ISendContext : IOutgoingContext
{
    public QueueName Destination { get; set; }
}
