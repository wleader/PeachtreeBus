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
    public abstract class BaseThread : IThread
    {
        private readonly string _name;
        private readonly int delayMs;
        private readonly ILog _log;
        private readonly IBusDataAccess _dataAccess;
        private readonly IProvideShutdownSignal _shutdown;

        public BaseThread(string name, int delayMs,
            ILog log,
            IBusDataAccess dataAccess,
            IProvideShutdownSignal shutdown)
        {
            _name = name;
            this.delayMs = delayMs;
            _log = log;
            _dataAccess = dataAccess;
            _shutdown = shutdown;
        }

        public abstract Task<bool> DoUnitOfWork();

        public async Task Run()
        {
            _log.Info($"Starting {_name} Thread.");

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
                    _log.Error(e.ToString());
                    _dataAccess.RollbackTransaction();
                }
            }
            while (!_shutdown.ShouldShutdown);

            _log.Info($"Shutdown {_name} Thread.");
        }
    }
}
