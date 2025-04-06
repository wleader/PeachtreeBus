using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscribedTask : IBaseTask;

public class CleanSubscribedTask(
    ICleanSubscribedTracker tracker,
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ILogger<CleanSubscribedTask> log)
    : BaseTask(dataAccess, log, "CleanSubscriptions")
    , ICleanSubscribedTask
{
    private readonly SubscriptionConfiguration? _configuration = configuration.SubscriptionConfiguration;
    private readonly ICleanSubscribedTracker _tracker = tracker;

    public override async Task<WorkResult> DoUnitOfWork()
    {
        if (_configuration is null) return new(false, false);

        var maxRows = _configuration.CleanMaxRows;
        var rows = await _dataAccess.ExpireSubscriptionMessages(maxRows);
        if (rows == 0) _tracker.WorkDone();
        return new WorkResult(true, rows > 0);
    }
}

