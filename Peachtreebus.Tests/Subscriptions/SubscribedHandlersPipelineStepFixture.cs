using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Peachtreebus.Tests.Sagas;
using PeachtreeBus;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peachtreebus.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedHandlersPipelineStepFixture
    {
        public class MessageWithoutInterface { };

        public class TestMessage : ISubscribedMessage { }

        private SubscribedHandlersPipelineStep _testSubject = default!;
        private Mock<IFindSubscribedHandlers> _findSubscribed = default!;

        [TestInitialize]
        public void Initialize()
        {
            _findSubscribed = new();
            _testSubject = new(_findSubscribed.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_MessageIsNotISubscribedMessage_Then_ThrowsUsefulException()
        {
            var context = new InternalSubscribedContext
            {
                Headers = new()
                {
                    MessageClass = typeof(MessageWithoutInterface).AssemblyQualifiedName!
                }
            };
            await _testSubject.Invoke(context, null);
        }


        [TestMethod]
        public async Task Given_MessageHasMultipleHandlers_When_Invoke_Then_AllHandlersAreInvoked()
        {
            var handler1 = new Mock<IHandleSubscribedMessage<TestMessage>>();
            var handler2 = new Mock<IHandleSubscribedMessage<TestMessage>>();

            List<IHandleSubscribedMessage<TestMessage>> handlers = [handler1.Object, handler2.Object];

            _findSubscribed.Setup(f => f.FindHandlers<TestMessage>()).Returns(handlers);

            var context = GetContext<TestMessage>();

            await _testSubject.Invoke(context, null);

            handler1.Verify(h => h.Handle(context, (TestMessage)context.Message), Times.Once);
            handler2.Verify(h => h.Handle(context, (TestMessage)context.Message), Times.Once);
        }

        /// <summary>
        /// Proves behavior when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(SubscribedMessageNoHandlerException))]
        public async Task Given_MessageHasNoHandlers_When_Invoke_Then_Throws()
        {
            _findSubscribed.Setup(f => f.FindHandlers<TestMessage>())
                .Returns(Array.Empty<IHandleSubscribedMessage<TestMessage>>());

            var context = GetContext<TestMessage>();

            await _testSubject.Invoke(context, null);
        }

        /// <summary>
        /// Proves behavior when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [ExpectedException(typeof(SubscribedMessageClassNotRecognizedException))]
        public async Task Given_AMessageContextWithAnUnrecognizedMessageType_When_Invoke_Then_Throws()
        {
            var context = CreateContextWithUnrecognizedMessageType();
            await _testSubject.Invoke(context, null);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingInterfaceException))]
        public async Task Given_AMessageThatDoesNotImplementIQueueMessage_When_Invoke_Then_Throws()
        {
            var context = GetContext<object>();// object does not implement IQueueMessage
            await _testSubject.Invoke(context, null);
        }

        private static IEnumerable<IHandleQueueMessage<TestSagaMessage1>> GetNoHandlers()
        {
            var list = new List<IHandleQueueMessage<TestSagaMessage1>>();
            return list;
        }

        private static InternalSubscribedContext GetContext<TMessage>()
            where TMessage : new()
        {
            var type = typeof(TMessage);
            return new InternalSubscribedContext()
            {
                Headers = new()
                {
                    MessageClass = type.FullName + ", " + type.Assembly.GetName().Name,
                },
                Message = new TMessage(),
                MessageData = new(),
            };
        }

        private static InternalSubscribedContext CreateContextWithUnrecognizedMessageType()
        {
            return new InternalSubscribedContext()
            {
                MessageData = new PeachtreeBus.Model.SubscribedMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers
                {
                    MessageClass = "Peachtreebus.Tests.Subscribed.NotARealMessageType, Peachtreebus.Tests"
                },
                Message = new TestSagaMessage1()
            };
        }

    }
}
