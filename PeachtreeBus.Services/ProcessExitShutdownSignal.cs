using System;

namespace PeachtreeBus.Services
{
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
