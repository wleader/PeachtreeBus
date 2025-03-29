using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeachtreeBus;

/// <summary>
/// Describes a piece of code that calls an IUnitOFWork in a loop until
/// signaled to stop.
/// </summary>
public interface IThread
{
    Task Run(CancellationToken cancellationToken);
}

/// <summary>
/// Describes something that is called repeatedly from the loop of
/// and IThread
/// </summary>
public interface IUnitOfWork
{
    Task<bool> DoWork();
}

/// <summary>
/// A basic thread that wrappes the Unit of Work in a database
/// Transaction.
/// </summary>
public abstract class BaseThread(
    string name,
    int delayMs,
    ILogger log,
    IBusDataAccess dataAccess)
    : IThread
{
    private readonly string _name = name;
    private readonly int delayMs = delayMs;
    private readonly ILogger _log = log;
    private readonly IBusDataAccess _dataAccess = dataAccess;

    public abstract Task<bool> DoUnitOfWork();

    public async Task Run(CancellationToken cancellationToken)
    {
        _log.BaseThread_ThreadStart(_name);
        do
        {
            try
            {
                // while reconnecting sounds expensive, its actually not.
                // the SqlClient library code does connection pooling for us.
                _dataAccess.Reconnect();
                _dataAccess.BeginTransaction();
                if (await DoUnitOfWork())
                {
                    _dataAccess.CommitTransaction();
                }
                else
                {
                    // there was no work to do.
                    // Rollback and go to sleep.
                    _dataAccess.RollbackTransaction();
                    if (!cancellationToken.IsCancellationRequested)
                        await Task.Delay(delayMs, cancellationToken);
                }
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
            }
        }
        while (!cancellationToken.IsCancellationRequested);

        _log.BaseThread_ThreadStop(_name);
    }
}
