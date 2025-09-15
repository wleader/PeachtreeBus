using System.Threading;

namespace PeachtreeBus.Tasks;

public interface IInterlockedCounter
{
    int Increment();
    int Decrement();
    int Value { get; }
}

public class InterlockedCounter : IInterlockedCounter
{
    private int _value = 0;
    public int Increment() => Interlocked.Increment(ref _value);
    public int Decrement() => Interlocked.Decrement(ref _value);
    public int Value
    {
        get => Interlocked.CompareExchange(ref _value, 0, 0);
        set => Interlocked.Exchange(ref _value, value);
    }
}
