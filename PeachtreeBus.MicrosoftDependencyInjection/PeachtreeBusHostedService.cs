using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeachtreeBus.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.MicrosoftDependencyInjection;

public class PeachtreeBusHostedService(
    IWrappedScopeFactory scopeFactory)
    : IHostedService, IDisposable
{
    private CancellationTokenSource _cts = default!;
    private ConfiguredTaskAwaitable _managerTask = default!;
    private IWrappedScope? _scope = default;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        await _managerTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = new();
        cancellationToken.Register(() => _cts.Cancel());

        _scope = scopeFactory.Create();

        var startupTasks = _scope.GetRequiredService<IEnumerable<IRunOnStartup>>();
        foreach (var startupTask in startupTasks)
        {
            await startupTask.Run().ConfigureAwait(false);
        }

        var manager = _scope.GetRequiredService<ITaskManager>();
        _managerTask = manager.Run(_cts.Token).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}