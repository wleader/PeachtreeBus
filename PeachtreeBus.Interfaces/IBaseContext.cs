namespace PeachtreeBus;

public interface IBaseContext
{
    public IWrappedScope? Scope { get; }
    public object Message { get; }
}
