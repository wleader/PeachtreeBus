namespace PeachtreeBus;

/// <summary>
/// Defines an interface that can be checked by service code to know when to shut down.
/// </summary>
public interface IProvideShutdownSignal
{
    bool ShouldShutdown { get; }
}
