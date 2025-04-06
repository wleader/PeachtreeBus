using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Subscriptions;

public interface ISubscriptionUpdateTask : IBaseTask;

public class SubscriptionUpdateTask(
    ISubscriptionUpdateTracker tracker,
    IBusDataAccess dataAccess,
    IBusConfiguration configuration,
    ISystemClock clock,
    ILogger<SubscriptionUpdateTask> log)
    : BaseTask(dataAccess, log, "SubscriptionsUpdate")
    , ISubscriptionUpdateTask
{
    private readonly ISubscriptionUpdateTracker _tracker = tracker;
    private readonly ISystemClock _clock = clock;
    private readonly SubscriptionConfiguration? _config = configuration.SubscriptionConfiguration;
    public override async Task<WorkResult> DoUnitOfWork()
    {
        if (_config is null) return new(false, false);
        var until = _clock.UtcNow.Add(_config.Lifespan);
        foreach (var topic in _config.Topics)
        {
            await _dataAccess.Subscribe(
                _config.SubscriberId,
                topic,
                until);
        }
        _tracker.WorkDone();
        return new(true, false);
    }
}
