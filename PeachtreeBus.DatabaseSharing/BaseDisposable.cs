using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.DatabaseSharing;

public abstract class BaseDisposable<T>(T resource)
    : IDisposable, IAsyncDisposable
    where T : IDisposable, IAsyncDisposable
{
    private long _disposed;
    public bool Disposed => Interlocked.Read(ref _disposed) != 0;

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        resource.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) return;
        await resource.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}