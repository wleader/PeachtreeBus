using System;

namespace PeachtreeBus.Tasks;

public interface ITaskCounter : IInterlockedCounter
{
    public int Available();
}

public class TaskCounter(
    IBusConfiguration configuration)
    : InterlockedCounter
    , ITaskCounter
{
    private readonly IBusConfiguration _configuration = configuration;
    public int Available() => Math.Max(0, _configuration.MessageConcurrency - Value);
}
