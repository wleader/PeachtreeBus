using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IDelayFactory
{
    Task Delay(TimeSpan delay, CancellationToken cancellationToken);
}

public class DelayFactory : IDelayFactory
{
    public Task Delay(TimeSpan delay, CancellationToken cancellationToken) =>
        delay == TimeSpan.Zero
            ? Task.CompletedTask
            : Task.Delay(delay, cancellationToken);
}
