﻿using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Data;
using System;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests
{
    /// <summary>
    /// Proves the behavior of BaseThread.
    /// </summary>
    [TestClass]
    public class BaseThreadTests
    {
        private class TestThread(
            string name,
            int delayMs,
            ILogger log,
            IBusDataAccess dataAccess,
            IProvideShutdownSignal shutdown)
            : BaseThread(name, delayMs, log, dataAccess, shutdown)
        {
            public bool UnitOfWorkResult { get; set; } = true;
            public bool Throw { get; set; } = false;

            public override Task<bool> DoUnitOfWork()
            {
                if (Throw) throw new Exception();
                return Task.FromResult(UnitOfWorkResult);
            }
        }

        private TestThread testThread = default!;
        private Mock<IProvideShutdownSignal> shutdown = default!;
        private int loopCount = 1;
        private Mock<IBusDataAccess> dataAccess = default!;
        private Mock<ILogger> log = default!;


        [TestInitialize]
        public void TestInitialize()
        {
            shutdown = new Mock<IProvideShutdownSignal>();

            shutdown.SetupGet(s => s.ShouldShutdown)
                .Returns(() =>
                {
                    return loopCount < 1;
                })
                .Callback(() =>
                {
                    loopCount--;
                });

            log = new Mock<ILogger>();

            dataAccess = new Mock<IBusDataAccess>();

            testThread = new TestThread("Test", 100, log.Object, dataAccess.Object, shutdown.Object);
        }

        /// <summary>
        /// Proves the thread loops forever until it receives the shutdown signal.
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
            loopCount = 0;
            await testThread.Run();
            dataAccess.Verify(d => d.BeginTransaction(), Times.Once);

            dataAccess.Invocations.Clear();
            loopCount = 1;
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
            loopCount = 0;
            await testThread.Run();
            dataAccess.Verify(d => d.CommitTransaction(), Times.Once);

            dataAccess.Invocations.Clear();
            loopCount = 1;
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
            loopCount = 0;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

            dataAccess.Invocations.Clear();
            loopCount = 1;
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
            loopCount = 0;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Once);

            dataAccess.Invocations.Clear();
            loopCount = 1;
            await testThread.Run();
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
        }

        [TestMethod]
        public async Task Run_ResetsDbConnection_When_WorkThrows_And_RollbackThrows()
        {
            testThread.Throw = true;

            dataAccess.Setup(d => d.RollbackTransaction()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));

            loopCount = 1;
            await testThread.Run();
            dataAccess.Verify(d => d.Reconnect(), Times.Exactly(2));

            dataAccess.Invocations.Clear();
            loopCount = 2;
            await testThread.Run();
            dataAccess.Verify(d => d.Reconnect(), Times.Exactly(3));
        }

        [TestMethod]
        public async Task Run_DoesNotBeginATransaction_When_DbResetThrows()
        {
            testThread.Throw = true;

            dataAccess.Setup(d => d.RollbackTransaction()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));
            dataAccess.Setup(d => d.Reconnect()).Throws(new InvalidOperationException("Unable to connect to database."));
            shutdown.SetupGet(s => s.ShouldShutdown).Returns(false);

            int counter = 0;
            dataAccess.Setup(d => d.Reconnect()).Callback(() =>
            {
                counter++;
                shutdown.SetupGet(s => s.ShouldShutdown).Returns(counter > 1);
            })
            .Throws(new InvalidOperationException("Unable to connect to database."));

            await testThread.Run();
            dataAccess.Verify(d => d.Reconnect(), Times.Exactly(2));
            dataAccess.Verify(d => d.BeginTransaction(), Times.Never);
            dataAccess.Verify(d => d.RollbackTransaction(), Times.Exactly(2));
        }
    }
}
