using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface ISleeper
{
    Task Sleep(int milliseconds);
    void Wake();
}

public class Sleeper : ISleeper
{
    private readonly object _lock = new();
    private TaskCompletionSource? _taskSource;

    public Task Sleep(int milliseconds)
    {
        lock (_lock)
        {
            _taskSource = new();
            Task.Delay(milliseconds)
                .ContinueWith((_) => Wake());
            return _taskSource.Task;
        }
    }

    public void Wake()
    {
        lock (_lock)
        {
            _taskSource?.SetResult();
            _taskSource = null;
        }
    }
}
