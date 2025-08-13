using Microsoft.Extensions.Hosting;
using PeachtreeBus.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class PeachtreeBusHostedService(
    IScopeFactory scopeFactory)
    : IHostedService, IDisposable
{
    private CancellationTokenSource _cts = default!;
    private Task _managerTask = default!;
    private IServiceProviderAccessor? _accessor = default;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        await _managerTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new();
        cancellationToken.Register(() => _cts.Cancel());

        _accessor = scopeFactory.Create();

        var startupRunner = _accessor.GetRequiredService<IRunStartupTasks>();
        startupRunner.RunStartupTasks();

        var manager = _accessor.GetRequiredService<ITaskManager>();
        _managerTask = manager.Run(_cts.Token);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Dispose();
        _accessor?.Dispose();
        GC.SuppressFinalize(this);
    }
}