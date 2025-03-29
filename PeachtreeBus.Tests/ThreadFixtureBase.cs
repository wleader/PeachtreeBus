using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests;

public abstract class ThreadFixtureBase<TThread>
    where TThread : BaseThread
{
    protected TThread _testSubject = default!;
    protected CancellationTokenSource _cts = new();

    protected void CancelToken() => _cts.Cancel();

    protected virtual async Task When_Run()
    {
        _cts = new();
        await _testSubject.Run(_cts.Token);
    }
}
