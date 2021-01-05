using System;
using System.Threading.Tasks;

namespace PeachtreeBus
{
    /// <summary>
    /// A Default Implementation of IIntervalRunner.
    /// </summary>
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

            // init the next run to in the past so immediately.
            DateTime nextrun = DateTime.UtcNow.AddSeconds(-1);

            while (!_provideShutdownSignal.ShouldShutdown)
            {

                if (DateTime.UtcNow > nextrun)
                {
                    // if its time to do the task, do it.
                    try
                    {
                        await runOnIntervalTask.Run();

                        // update the next run time.
                        // this is from now, not from when the task was scheduled so in reality
                        // the time between the starts of the task is actually slightly longer 
                        // once you include the task time itself.
                        // this is good enough for most purposes. This isn't mean to be a a 
                        // precise timer type thing.
                        nextrun = DateTime.UtcNow.AddMilliseconds(runOnIntervalTask.SuccessWaitMs);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"There was an error running Interval Task {runOnIntervalTask.Name}. {ex}");
                        // the task failed. update  the next turn.
                        // the task is allowed to specify a different wait when there is a failure.
                        nextrun = DateTime.UtcNow.AddMilliseconds(runOnIntervalTask.ErrorWaitMs);
                    }
                }

                // calculate a delay that is no more than 100ms. That way the loop doesn't sleep any more
                // than 100ms, so if the shutdown signal happens, shutdown isn't left waiting.
                var delay = Math.Min(100, (int)nextrun.Subtract(DateTime.UtcNow).TotalMilliseconds);
                await Task.Delay(delay);
            }
        }
    }
}
