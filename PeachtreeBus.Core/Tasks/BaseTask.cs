using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus.Tasks;

public interface IBaseTask
{
    Task Run(CancellationToken token);
}

public abstract class BaseTask(
    IBusDataAccess dataAccess,
    ILogger log,
    string name)
{
    protected readonly IBusDataAccess _dataAccess = dataAccess;
    private readonly ILogger _log = log;
    private readonly string _name = name;

    public readonly record struct WorkResult(bool Commit, bool RunAgain);

    public abstract Task<WorkResult> DoUnitOfWork();

    public async Task Run(CancellationToken token)
    {
        do
        {
            try
            {
                // while reconnecting sounds expensive, its actually not.
                // the SqlClient library code does connection pooling for us.
                _dataAccess.Reconnect();
                _dataAccess.BeginTransaction();
                var result = await DoUnitOfWork();

                if (result.Commit)
                {
                    _dataAccess.CommitTransaction();
                }
                else
                {
                    _dataAccess.RollbackTransaction();
                }

                if (!result.RunAgain)
                    break;
            }
            catch (Exception e)
            {
                _log.BaseThread_ThreadError(_name, e);
                try
                {
                    _dataAccess.RollbackTransaction();
                }
                catch (Exception rollbackEx)
                {
                    _log.BaseThread_RollbackFailed(_name, rollbackEx);
                }
                break;
            }
        }
        while (!token.IsCancellationRequested);
    }
}
