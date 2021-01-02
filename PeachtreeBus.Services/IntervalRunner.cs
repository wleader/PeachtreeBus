using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Services
{
    public class IntervalRunner : IIntervalRunner
    {
        private readonly IProvideShutdownSignal _provideShutdownSignal;
        private readonly ILog<IntervalRunner> _log;

        public IntervalRunner(IProvideShutdownSignal provideShutdownSignal, ILog<IntervalRunner> log)
        {
            _log = log;
            _provideShutdownSignal = provideShutdownSignal;
        }

        public async Task Run(IRunOnIntervalTask runOnIntervalTask)
        {
            _log.Info($"Starting Interval Task {runOnIntervalTask.Name}");

            DateTime nextrun = DateTime.UtcNow.AddSeconds(-1);

            while (!_provideShutdownSignal.ShouldShutdown)
            {
                if (DateTime.UtcNow > nextrun)
                {
                    try
                    {
                        await runOnIntervalTask.Run();
                        nextrun = DateTime.UtcNow.AddMilliseconds(runOnIntervalTask.SuccessWaitMs);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"There was an error running Interval Task {runOnIntervalTask.Name}. {ex}");
                        nextrun = DateTime.UtcNow.AddMilliseconds(runOnIntervalTask.ErrorWaitMs);
                    }
                }
                var delay = Math.Min(100, (int)nextrun.Subtract(DateTime.UtcNow).TotalMilliseconds);
                await Task.Delay(delay);
            }
        }
    }
}
