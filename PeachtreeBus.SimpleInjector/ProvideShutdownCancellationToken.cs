using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace PeachtreeBus.SimpleInjector;

/// <summary>
/// Defines an interface that can be checked by service code to know when to shut down.
/// </summary>
public interface IProvideShutdownCancellationToken
{
    CancellationToken GetCancellationToken();
}

/// <summary>
/// An Implementation of IProvideShutdownCancellationToken that uses
/// The Current AppDomain's ProcessExit event.
/// </summary>
public class ProvideShutdownCancellationToken : IProvideShutdownCancellationToken
{
    private readonly CancellationTokenSource _cts = new();

    public CancellationToken GetCancellationToken() => _cts.Token;

    public void SignalShutdown() => _cts.Cancel();

    public ProvideShutdownCancellationToken()
    {
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
    }

    [ExcludeFromCodeCoverage(Justification = "Would require an abstraction around AppDomain.CurrentDomain which itself would be untestable.")]
    ~ProvideShutdownCancellationToken()
    {
        AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
    }

    [ExcludeFromCodeCoverage(Justification = "Would require an abstraction around AppDomain.CurrentDomain which itself would be untestable.")]
    private void CurrentDomain_ProcessExit(object? sender, EventArgs e) => _cts.Cancel();
}
