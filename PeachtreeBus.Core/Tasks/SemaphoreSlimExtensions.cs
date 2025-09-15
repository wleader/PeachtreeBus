using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public static class SemaphoreSlimExtensions
{
    public static void Invoke(this SemaphoreSlim semaphore, Action? action)
    {
        semaphore.Wait();
        try
        {
            action?.Invoke();
        }
        finally
        {
            semaphore.Release();
        }
    }

    public static T Invoke<T>(this SemaphoreSlim semaphore, Func<T> func)
    {
        semaphore.Wait();
        try
        {
            return func.Invoke();
        }
        finally
        {
            semaphore.Release();
        }
    }
}
