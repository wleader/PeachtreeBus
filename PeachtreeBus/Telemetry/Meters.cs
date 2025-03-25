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
}

public class Meters : IMeters
{
    public static readonly Meter Meter = new("PeachtreeBus", "0.11.0");

    public const string UnitMessages = "Messages";

    public static readonly Counter<long> CompletedMessageCount =
        Meter.CreateCounter<long>(nameof(CompletedMessageCount),
            UnitMessages,
            "A count of successfully processed messages.");
    
    public static readonly UpDownCounter<int> ActiveMessageCount =
        Meter.CreateUpDownCounter<int>(nameof(ActiveMessageCount),
            UnitMessages,
            "The number of messages that are currently being handled.");

    public static readonly Counter<long> AttemptedMessageCount =
        Meter.CreateCounter<long>(nameof(AttemptedMessageCount),
            UnitMessages,
            "A count of attempts to handle a message.");

    public static readonly Counter<long> FailedMessageCount =
        Meter.CreateCounter<long>(nameof(FailedMessageCount),
            UnitMessages,
            "A count of messages sent to the Failed queue.");

    public static readonly Counter<long> SentMessageCount =
        Meter.CreateCounter<long>(nameof(SentMessageCount),
            UnitMessages,
            "A count of messages sent.");

    public static readonly Counter<long> RetryMessageCount =
        Meter.CreateCounter<long>(nameof(RetryMessageCount),
            UnitMessages,
            "A count of messages that got scheduled for a retry.");

    public static readonly Counter<long> LockedSagaMessageCount =
        Meter.CreateCounter<long>(nameof(LockedSagaMessageCount),
            UnitMessages,
            "A count of times when a message was re-queued because it's saga was locked.");

    public void CompleteMessage() => CompletedMessageCount.Add(1);

    public void FailMessage() => FailedMessageCount.Add(1);

    public void FinishMessage() => ActiveMessageCount.Add(-1);

    public void SentMessage(long count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count, nameof(count));
        SentMessageCount.Add(count);
    }

    public void RetryMessage() => RetryMessageCount.Add(1);

    public void SagaBlocked() => LockedSagaMessageCount.Add(1);

    public void StartMessage()
    {
        ActiveMessageCount.Add(1);
        AttemptedMessageCount.Add(1);
    }
}
