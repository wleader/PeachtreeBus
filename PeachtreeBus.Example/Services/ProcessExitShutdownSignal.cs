using System;
using System.Threading;

namespace PeachtreeBus.Example.Services;

/// <summary>
/// An Implementation of IProvideShutdownSignal that uses
/// The Current AppDomain's ProcessExit event.
/// </summary>
public class ProcessExitShutdownSignal : IProvideShutdownSignal
{
    private readonly CancellationTokenSource _cts = new();

    public CancellationToken GetCancellationToken() => _cts.Token;

    public void SignalShutdown() => _cts.Cancel();

    public ProcessExitShutdownSignal()
    {
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }

    ~ProcessExitShutdownSignal()
    {
        AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
    }

    private void CurrentDomain_ProcessExit(object? sender, EventArgs e) => _cts.Cancel();
}
