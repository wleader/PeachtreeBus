using System;

namespace PeachtreeBus.Services
{

    /// <summary>
    /// An Implementation of IProvideShutdownSignal that uses
    /// The Current AppDomain's ProcessExit event.
    /// </summary>
    public class ProcessExitShutdownSignal : IProvideShutdownSignal
    {
        public bool ShouldShutdown { get; private set; }

        public ProcessExitShutdownSignal()
        {
            ShouldShutdown = false;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ShouldShutdown = true;
        }
    }
}
