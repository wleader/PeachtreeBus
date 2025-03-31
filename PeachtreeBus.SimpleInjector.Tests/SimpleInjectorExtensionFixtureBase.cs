using Microsoft.Extensions.Logging;
using Moq;
using PeachtreeBus.DatabaseSharing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace PeachtreeBus.SimpleInjector.Tests
{
    public abstract class SimpleInjectorExtensionFixtureBase
    {
        protected Container _container = default!;
        protected ILoggerFactory _loggerFactory = default!;
        private readonly Mock<IProvideDbConnectionString> _provideDBConnectionString = new();
        private Mock<IProvideShutdownSignal> _provideShutdownSignal = default!;
        protected List<Assembly> _assemblies = default!;
        private CancellationTokenSource _cts = new();

        [TestInitialize]
        public void Intialize()
        {
            _cts = new();
            _assemblies = [Assembly.GetExecutingAssembly()];

            _container = new Container();
            _container.Options.AllowOverridingRegistrations = true;
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            // users of peachtree bus must provide their own logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSimpleConsole();
            });

            // users must provide their own way of configuring the connection string.
            _container.RegisterInstance(_provideDBConnectionString.Object);

            // users must provide their own shutdown signal.
            // provide one that immediatly shuts down so the tests complete.
            _provideShutdownSignal = new();
            _provideShutdownSignal.Setup(p => p.GetCancellationToken())
                .Returns(_cts.Token);
            _cts.Cancel();
            _container.RegisterInstance(_provideShutdownSignal.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerFactory.Dispose();
        }
    }
}
