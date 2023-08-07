using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Data;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Queues
{
    [TestClass]
    public class QueueHandlersPipelineStepFixture
    {
        public class MessageWithoutInterface { };

        private QueueHandlersPipelineStep _testSubject;
        private Mock<IFindQueueHandlers> _findHandlers;
        private Mock<ILogger<QueueHandlersPipelineStep>> _log;
        private Mock<ISagaMessageMapManager> _sagaMessageMapManager;
        private Mock<IQueueReader> _queueReader;
        private Mock<IPerfCounters> _perfCounters;
        private Mock<IBusDataAccess> _dataAccess;
        private QueueContext _context;

        [TestInitialize] 
        public void Initialize()
        {
            _findHandlers = new();
            _log = new();
            _sagaMessageMapManager = new();
            _queueReader = new();
            _perfCounters = new();
            _dataAccess = new();

            _context = new();
            _context.Headers = new();

            _testSubject = new(
                "TestQueue",
                _findHandlers.Object,
                _log.Object,
                _sagaMessageMapManager.Object,
                _queueReader.Object,
                _perfCounters.Object,
                _dataAccess.Object,
                "TestSavePoint");
        }


        private Task Next(QueueContext context)
        {
            return Task.CompletedTask;
        }


        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_MessageIsNotIQueuedMessage_Then_ThrowsUsefulException()
        {
            _context.Headers.MessageClass = typeof(MessageWithoutInterface).AssemblyQualifiedName;
            await _testSubject.Invoke(_context, Next);
        }
    }
}
