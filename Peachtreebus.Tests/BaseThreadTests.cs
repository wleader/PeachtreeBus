using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace Peachtreebus.Tests
{
    /// <summary>
    /// Proves the behavior of BaseThread.
    /// </summary>
    [TestClass]
    public class BaseThreadTests
    {
        private class TestThread : BaseThread
        {
            public TestThread(string name, int delayMs, ILogger log,
                PeachtreeBus.Data.IBusDataAccess dataAccess, IProvideShutdownSignal shutdown)
                : base(name, delayMs, log, dataAccess, shutdown)
            { }

            public bool UnitOfWorkResult { get; set; } = true;
            public bool Throw { get; set; } = false;

            public override Task<bool> DoUnitOfWork()
            {
                if (Throw) throw new Exception();
                return Task.FromResult(UnitOfWorkResult);
            }
        }

        private TestThread testThread;
        private Mock<IProvideShutdownSignal> shutdown;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess;
        private Mock<ILogger> log;


        [TestInitialize]
        public void TestInitialize()
        {
            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() => loopCount > 0)
                .Callback(() => loopCount--);

            log = new Mock<ILogger>();

            dataAccess = new Mock<IBusDataAccess>();

            testThread = new TestThread("Test", 100, log.Object, dataAccess.Object, shutdown.Object);
        }

        /// <summary>
        /// Proves the thread loops forever until it recieves the shutdown signal.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_LoopsUntilShutdownSignal()
        {
            shutdown.SetupGet(s => s.ShouldShutdown).Returns(false);
            var t = Task.Run(() => testThread.Run());

            await Task.Delay(10);
            Assert.IsFalse(t.IsCompleted);
            await Task.Delay(10);
            Assert.IsFalse(t.IsCompleted);

            shutdown.SetupGet(s => s.ShouldShutdown).Returns(true);

            await Task.Delay(10);
            Assert.IsTrue(t.IsCompleted);

            await t;
        }

        /// <summary>
        /// Proves that unit of work inside the loop is wrapped
        /// in a database transaction.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_BeginsTransaction()
        {
            await testThread.Run();
            dataAccess.Verify(d => d.BeginTransaction(), Times.Once);

            loopCount = 2;
            await testThread.Run();
            dataAccess.Verify(d => d.BeginTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Proves that the DB transaction is committed when the 
        /// unit of work reports success.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_CommitsWhenWorkReturnsTrue()
        {
            testThread.UnitOfWorkResult = true;
            await testThread.Run();
            dataAccess.Verify(d => d.CommitTransaction(), Times.Once);

            loopCount = 2;
            await testThread.Run();
            dataAccess.Verify(d => d.CommitTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Proves that the DB transaction is rolled back when the
        /// unit of work reports failure.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_RollsBackWhenWorkReturnsFalse()
        {
            testThread.UnitOfWorkResult = false;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

            loopCount = 2;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
        }

        /// <summary>
        /// Proves that DB transaction is rolled back when the
        /// unit of work throws an exception.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Run_RollsBackWhenWorkThrows()
        {
            testThread.Throw = true;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

            loopCount = 2;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
        }
    }
}
