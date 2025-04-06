using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using PeachtreeBus.Tasks;
using System.Threading.Tasks;

namespace PeachtreeBus.Queues;

public interface IProcessQueuedTask : IBaseTask;

public class ProcessQueuedTask(
    IBusConfiguration configuration,
    ILogger<ProcessQueuedTask> log,
    IBusDataAccess dataAccess,
    IQueueWork queueWork)
    : BaseTask(dataAccess, log, "ProcessQueued")
    , IProcessQueuedTask
{
    private readonly QueueConfiguration? _configuration = configuration.QueueConfiguration;
    private readonly IQueueWork queueWork = queueWork;
    public override async Task<WorkResult> DoUnitOfWork()
    {
        if (_configuration is null) return new(false, false);
        queueWork.QueueName = _configuration.QueueName;
        var result = await queueWork.DoWork();
        return new(result, result);
    }
}
