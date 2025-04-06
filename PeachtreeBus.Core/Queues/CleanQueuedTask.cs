using Microsoft.Extensions.Logging;
using PeachtreeBus.Cleaners;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface ICleanQueuedTask : IBaseTask;

public class CleanQueuedTask(
    ICleanQueuedTracker tracker,
    IQueueCleanupWork cleanupWork,
    IBusDataAccess dataAccess,
    ILogger<CleanQueuedTask> log)
    : BaseTask(dataAccess, log, "CleanQueued")
    , ICleanQueuedTask
{
    private readonly ICleanQueuedTracker _tracker = tracker;

    public override async Task<WorkResult> DoUnitOfWork()
    {
        var result = await cleanupWork.DoWork();
        _tracker.WorkDone();
        return new(result, result);
    }
}
