using Microsoft.Extensions.Logging;
using Moq;
using PeachtreeBus.DatabaseSharing;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System.Collections.Generic;
using System.Reflection;

namespace PeachtreeBus.SimpleInjector.Tests
{
    public abstract class SimpleInjectorExtensionFixtureBase
    {
        protected Container _container = default!;
        protected ILoggerFactory _loggerFactory = default!;
        private readonly Mock<IProvideDbConnectionString> _provideDBConnectionString = new();
        private Mock<IProvideShutdownSignal> _provideShutdownSignal = default!;
        protected List<Assembly> _assemblies = default!;

        [TestInitialize]
        public void Intialize()
        {
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
            _provideShutdownSignal.SetupGet(p => p.ShouldShutdown)
                .Returns(() => true);
            _container.RegisterInstance(_provideShutdownSignal.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerFactory.Dispose();
        }
    }
}
