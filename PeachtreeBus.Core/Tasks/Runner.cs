using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IRunner
{
    Task RunRepeatedly(CancellationToken token);
}

public abstract class Runner<TBaseTask>(
    IBusDataAccess dataAccess,
    ILogger<Runner<TBaseTask>> log,
    TBaseTask task)
    where TBaseTask : IBaseTask
{
    protected readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ILogger<Runner<TBaseTask>> _log = log;
    private readonly TBaseTask _task = task;

    public async Task RunRepeatedly(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                // while reconnecting sounds expensive, its actually not.
                // the SqlClient library code does connection pooling for us.
                _dataAccess.Reconnect();
                _dataAccess.BeginTransaction();
                var result = await _task.RunOne();

                if (result)
                {
                    _dataAccess.CommitTransaction();
                }
                else
                {
                    _dataAccess.RollbackTransaction();
                    break;
                }
            }
            catch (Exception e)
            {
                _log.TaskException(e);
                try
                {
                    _dataAccess.RollbackTransaction();
                }
                catch (Exception rollbackEx)
                {
                    _log.RollbackFailed(rollbackEx);
                }
                break;
            }
        }
    }
}
