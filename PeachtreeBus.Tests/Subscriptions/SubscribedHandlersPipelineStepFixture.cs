using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Tests.Sagas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Tests.Subscriptions
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
        public async Task Given_MessageIsNotISubscribedMessage_Then_ThrowsUsefulException()
        {
            var context = new InternalSubscribedContext
            {
                Headers = new()
                {
                    MessageClass = typeof(MessageWithoutInterface).AssemblyQualifiedName!
                }
            };
            await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
                _testSubject.Invoke(context, null));
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
                .Returns([]);

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
        public async Task Given_AMessageThatDoesNotImplementISubscribedMessage_When_Invoke_Then_Throws()
        {
            var context = GetContext<object>();// object does not implement IQueueMessage
            await Assert.ThrowsExceptionAsync<TypeIsNotISubscribedMessageException>(() =>
                _testSubject.Invoke(context, null));
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
                MessageData = new SubscribedMessage
                {
                    MessageId = Guid.NewGuid(),
                },
                Headers = new Headers
                {
                    MessageClass = "PeachtreeBus.Tests.Subscribed.NotARealMessageType, PeachtreeBus.Tests"
                },
                Message = new TestSagaMessage1()
            };
        }

    }
}
