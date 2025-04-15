using System.Threading;

namespace PeachtreeBus.Tasks;

public interface IInterlockedCounter
{
    void Increment();
    void Decrement();
    int Value { get; }
}
public class InterlockedCounter
{
    private int _value = 0;
    public void Increment() => Interlocked.Increment(ref _value);
    public void Decrement() => Interlocked.Decrement(ref _value);
    public int Value
    {
        get => Interlocked.CompareExchange(ref _value, 0, 0);
        set => Interlocked.Exchange(ref _value, value);
    }
}
