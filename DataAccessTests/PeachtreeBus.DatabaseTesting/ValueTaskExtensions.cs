using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseTesting;

public static class ValueTaskExtensions
{
    public static void Wait(this ValueTask task)
    {
        if (task.IsCompletedSuccessfully)
            return;
        task.AsTask().GetAwaiter().GetResult();
    }
}
