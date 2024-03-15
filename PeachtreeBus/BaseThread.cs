using Microsoft.Extensions.Logging;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// Describes a piece of code that calls an IUnitOFWork in a loop until
    /// signaled to stop.
    /// </summary>
    public interface IThread
    {
        Task Run();
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
        IBusDataAccess dataAccess,
        IProvideShutdownSignal shutdown)
        : IThread
    {
        private readonly string _name = name;
        private readonly int delayMs = delayMs;
        private readonly ILogger _log = log;
        private readonly IBusDataAccess _dataAccess = dataAccess;
        private readonly IProvideShutdownSignal _shutdown = shutdown;

        public abstract Task<bool> DoUnitOfWork();

        public async Task Run()
        {
            _log.BaseThread_ThreadStart(_name);

            do
            {
                try
                {
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
                        await Task.Delay(delayMs);
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
                        _log.BaseThread_ResettingDbConnection();
                        _dataAccess.Reset();
                    }
                }
            }
            while (!_shutdown.ShouldShutdown);

            _log.BaseThread_ThreadStop(_name);
        }
    }
}
