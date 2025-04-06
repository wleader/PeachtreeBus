using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface IProcessSubscribedTask : IBaseTask;

public class ProcessSubscribedTask(
    IBusDataAccess dataAccess,
    ILogger<ProcessSubscribedTask> log,
    ISubscribedWork work,
    IBusConfiguration configuration)
    : BaseTask(dataAccess, log, nameof(ProcessSubscribedTask))
    , IProcessSubscribedTask
{
    private readonly ISubscribedWork work = work;
    private readonly SubscriptionConfiguration? _configuration = configuration.SubscriptionConfiguration;

    public override async Task<WorkResult> DoUnitOfWork()
    {
        if (_configuration is null) return new(false, false);
        work.SubscriberId = _configuration.SubscriberId;
        var result = await work.DoWork();
        return new(result, result);
    }
}
