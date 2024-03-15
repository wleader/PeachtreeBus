using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedHandlersPipelineStepFixture
    {
        public class MessageWithoutInterface { };

        private SubscribedHandlersPipelineStep _testSubject;
        private Mock<IFindSubscribedHandlers> _findSubscribed;
        private InternalSubscribedContext _context;
        
        [TestInitialize]
        public void Initialize()
        {
            _context = new();
            _context.Headers = new();
            _findSubscribed = new();
            _testSubject = new(_findSubscribed.Object);
        }

        private Task Next(SubscribedContext context)
        {
            return Task.CompletedTask;
        }

        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_MessageIsNotISubscribedMessage_Then_ThrowsUsefulException()
        {
            _context.Headers.MessageClass = typeof(MessageWithoutInterface).AssemblyQualifiedName;
            await _testSubject.Invoke(_context, Next);
        }
    }
}
