﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Exceptions;
using PeachtreeBus.Subscriptions;
using PeachtreeBus.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Subscriptions
{
    [TestClass]
    public class SubscribedPipelineFinalStepFixture
    {
        public class MessageWithoutInterface { };

        public class TestMessage : ISubscribedMessage { }

        private SubscribedPipelineFinalStep _testSubject = default!;
        private readonly FakeServiceProviderAccessor _accessor = new(new());
        private readonly ClassNameService _classNameService = new();

        private IEnumerable<IHandleSubscribedMessage<TestMessage>> _handlers = [];

        [TestInitialize]
        public void Initialize()
        {
            _accessor.Reset();

            _accessor.Add(() => _handlers);

            _testSubject = new(
                _accessor.Object,
                _classNameService);
        }

        [TestMethod]
        public async Task Given_MessageIsNotISubscribedMessage_Then_ThrowsUsefulException()
        {
            var context = TestData.CreateSubscribedContext(
                headers: new() { MessageClass = typeof(MessageWithoutInterface).GetClassName() });
            await Assert.ThrowsExactlyAsync<TypeIsNotISubscribedMessageException>(() =>
                _testSubject.Invoke(context, null));
        }


        [TestMethod]
        public async Task Given_MessageHasMultipleHandlers_When_Invoke_Then_AllHandlersAreInvoked()
        {
            var handler1 = new Mock<IHandleSubscribedMessage<TestMessage>>();
            var handler2 = new Mock<IHandleSubscribedMessage<TestMessage>>();

            _handlers = [handler1.Object, handler2.Object];

            var context = TestData.CreateSubscribedContext(userMessage: new TestMessage());

            await _testSubject.Invoke(context, null);

            handler1.Verify(h => h.Handle(context, (TestMessage)context.Message), Times.Once);
            handler2.Verify(h => h.Handle(context, (TestMessage)context.Message), Times.Once);
        }

        /// <summary>
        /// Proves behavior when a message has no handlers.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_MessageHasNoHandlers_When_Invoke_Then_Throws()
        {
            _handlers = [];

            var context = TestData.CreateSubscribedContext(userMessage: new TestMessage());

            await Assert.ThrowsExactlyAsync<SubscribedMessageNoHandlerException>(() =>
                _testSubject.Invoke(context, null));
        }

        /// <summary>
        /// Proves behavior when message type unrecognized.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Given_AMessageContextWithAnUnrecognizedMessageType_When_Invoke_Then_Throws()
        {
            var context = TestData.CreateSubscribedContext(
                headers: TestData.CreateHeadersWithUnrecognizedMessageClass());
            await Assert.ThrowsExactlyAsync<SubscribedMessageClassNotRecognizedException>(() =>
                _testSubject.Invoke(context, null));
        }

        [TestMethod]
        public async Task Given_AMessageThatDoesNotImplementISubscribedMessage_When_Invoke_Then_Throws()
        {
            var context = TestData.CreateSubscribedContext(userMessage: new object());// object does not implement IQueueMessage
            await Assert.ThrowsExactlyAsync<TypeIsNotISubscribedMessageException>(() =>
                _testSubject.Invoke(context, null));
        }

        [TestMethod]
        public async Task Given_FindHandlersReturnsNull_When_Invoke_Then_Throws()
        {
            _handlers = null!;

            var context = TestData.CreateSubscribedContext(userMessage: new TestMessage());

            await Assert.ThrowsExactlyAsync<SubscribedMessageNoHandlerException>(() =>
                _testSubject.Invoke(context, null));
        }
    }
}
