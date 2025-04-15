using System.Collections.Generic;

namespace PeachtreeBus.Core.Tests;

public static class Extensions
{
    public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> values)
    {
        foreach (var item in values) { queue.Enqueue(item); }
    }
}
