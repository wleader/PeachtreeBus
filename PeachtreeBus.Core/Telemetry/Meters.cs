using System;
using System.Diagnostics.Metrics;

namespace PeachtreeBus.Telemetry;

public interface IMeters
{
    void StartMessage();
    void FinishMessage();
    void SagaBlocked();
    void RetryMessage();
    void CompleteMessage();
    void FailMessage();
    void SentMessage(long count);
    void StartTask();
    void EndTask();
}

public class Meters : IMeters
{
    public static readonly Meter Messaging = new("PeachtreeBus.Messaging", "0.12.5");

    public const string UnitMessages = "messages";
    public const string UnitTasks = "tasks";

    public static readonly Counter<long> CompletedMessageCount =
        Messaging.CreateCounter<long>("messaging.client.consumed.messages",
            UnitMessages,
            "A count of successfully processed messages.");
    
    public static readonly UpDownCounter<int> ActiveMessageCount =
        Messaging.CreateUpDownCounter<int>("peachtreebus.client.active.messages",
            UnitMessages,
            "The number of messages that are currently being handled.");

    public static readonly Counter<long> AttemptedMessageCount =
        Messaging.CreateCounter<long>("peachtreebus.client.attempted.messages",
            UnitMessages,
            "A count of attempts to handle a message.");

    public static readonly Counter<long> FailedMessageCount =
        Messaging.CreateCounter<long>("peachtreebus.client.failed.messages",
            UnitMessages,
            "A count of messages sent to the Failed queue.");

    public static readonly Counter<long> SentMessageCount =
        Messaging.CreateCounter<long>("messaging.client.sent.messages",
            UnitMessages,
            "A count of messages sent.");

    public static readonly Counter<long> RetryMessageCount =
        Messaging.CreateCounter<long>("peachtreebus.client.retry.messages",
            UnitMessages,
            "A count of messages that got scheduled for a retry.");

    public static readonly Counter<long> BlockedSagaMessageCount =
        Messaging.CreateCounter<long>("peachtreebus.client.blockedsaga.messsages",
            UnitMessages,
            "A count of times when a message was re-queued because it's saga was locked.");

    public static readonly UpDownCounter<int> ActiveTaskCount =
        Messaging.CreateUpDownCounter<int>("peachtreebus.client.active.tasks",
            UnitTasks,
            "A count of all tasks that have started and have not completed.");

    public void CompleteMessage() => CompletedMessageCount.Add(1);

    public void FailMessage() => FailedMessageCount.Add(1);

    public void FinishMessage() => ActiveMessageCount.Add(-1);

    public void SentMessage(long count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        SentMessageCount.Add(count);
    }

    public void RetryMessage() => RetryMessageCount.Add(1);

    public void SagaBlocked() => BlockedSagaMessageCount.Add(1);

    public void StartMessage()
    {
        ActiveMessageCount.Add(1);
        AttemptedMessageCount.Add(1);
    }

    public void StartTask() => ActiveTaskCount.Add(1);
    public void EndTask() => ActiveTaskCount.Add(-1);
}
