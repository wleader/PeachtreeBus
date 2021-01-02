namespace PeachtreeBus
{
    public interface IProvideShutdownSignal
    {
        bool ShouldShutdown { get; }
    }
}
