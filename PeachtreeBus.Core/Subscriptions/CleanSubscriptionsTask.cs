using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ICleanSubscriptionsTask : IBaseTask;

public class CleanSubscriptionsTask(
    ICleanSubscriptionsTracker tracker,
    IBusConfiguration configuration,
    IBusDataAccess dataAccess,
    ILogger<CleanSubscriptionsTask> log)
    : BaseTask(dataAccess, log, "CleanSubscriptions")
    , ICleanSubscriptionsTask
{
    private readonly SubscriptionConfiguration? _configuration = configuration.SubscriptionConfiguration;
    private readonly ICleanSubscriptionsTracker _tracker = tracker;

    public override async Task<WorkResult> DoUnitOfWork()
    {
        if (_configuration is null) return new(false, false);
        var maxRows = _configuration.CleanMaxRows;
        var rows = await _dataAccess.ExpireSubscriptions(maxRows);
        if (rows == 0) _tracker.WorkDone();
        return new WorkResult(true, rows > 0);
    }
}
