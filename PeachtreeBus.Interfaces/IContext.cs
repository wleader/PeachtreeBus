namespace PeachtreeBus;

/// <summary>
/// Context information that is common to all context types.
/// </summary>
public interface IContext
{
    public IWrappedScope? Scope { get; }
    public object Message { get; }
}
