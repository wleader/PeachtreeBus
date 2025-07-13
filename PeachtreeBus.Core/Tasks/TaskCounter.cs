using System;

namespace PeachtreeBus.Tasks;

public interface ITaskCounter : IInterlockedCounter
{
    public int Available();
}

public interface IMessagingTaskCounter : ITaskCounter;

public class MessagingTaskCounter(
    IBusConfiguration configuration)
    : InterlockedCounter
    , IMessagingTaskCounter
{
    private readonly IBusConfiguration _configuration = configuration;
    public int Available() => Math.Max(0, _configuration.MessageConcurrency - Value);
}

public interface IScheduledTaskCounter : ITaskCounter;

public class ScheduledTaskCounter : IScheduledTaskCounter
{
    public int Value => 0;

    public int Available() => 1;

    public void Decrement() { }

    public void Increment() { }
}
