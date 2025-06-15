using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PeachtreeBus.Core.Tests.Sagas
{
    /// <summary>
    /// Proves the behavior of SagaTypeExtensions.
    /// </summary>
    [TestClass]
    public class SagaTypeExtensionsFixture
    {
        private class NotASagaStartMessage : IQueueMessage { }
        private class SagaStartMessage : IQueueMessage { }

        [ExcludeFromCodeCoverage]
        private class Saga1 : Saga<object>,
            IHandleQueueMessage<NotASagaStartMessage>,
            IHandleSagaStartMessage<SagaStartMessage>
        {
            public override SagaName SagaName => throw new NotImplementedException();

            public override void ConfigureMessageKeys(ISagaMessageMap mapper)
            {
                throw new NotImplementedException();
            }

            public Task Handle(IQueueContext context, SagaStartMessage message)
            {
                throw new NotImplementedException();
            }

            public Task Handle(IQueueContext context, NotASagaStartMessage message)
            {
                throw new NotImplementedException();
            }
        }

        private class Saga2 : Saga1 { }

        private class TotallyNotASaga { }

        private class StillNotASaga : TotallyNotASaga { }

        private class GenericNotASaga<T> { }

        private class InheritsGenericNotASaga<T> : GenericNotASaga<T> { }

        /// <summary>
        /// Proves when directly inehriting saga
        /// </summary>
        [TestMethod]
        public void IsSubClassOfSaga_WhenDirectlySaga()
        {
            Assert.IsTrue(typeof(Saga1).IsSubclassOfSaga());
        }

        /// <summary>
        /// proves when indirectly ineheriting saga
        /// </summary>
        [TestMethod]
        public void IsSubClassOfSaga_WhenIndirectlySaga()
        {
            Assert.IsTrue(typeof(Saga2).IsSubclassOfSaga());
        }

        /// <summary>
        /// proves when not a saga
        /// </summary>
        [TestMethod]
        public void IsSubClassOfSaga_WhenNotSaga()
        {
            Assert.IsFalse(typeof(TotallyNotASaga).IsSubclassOfSaga());
            Assert.IsFalse(typeof(StillNotASaga).IsSubclassOfSaga());
        }

        /// <summary>
        /// proves when a saga start message
        /// </summary>
        [TestMethod]
        public void IsSagaStartHandler_WhenAStartMessage()
        {
            Assert.IsTrue(typeof(Saga1).IsSagaStartHandler(typeof(SagaStartMessage)));
        }

        /// <summary>
        /// poves when not a start message
        /// </summary>
        [TestMethod]
        public void IsSagaStartHandler_WhenNotAStartMessage()
        {
            Assert.IsFalse(typeof(Saga1).IsSagaStartHandler(typeof(NotASagaStartMessage)));
        }


        /// <summary>
        /// proves when not a saga
        /// </summary>
        [TestMethod]
        public void IsSubClassOfSaga_WhenGenericAndNotSaga()
        {
            Assert.IsFalse(typeof(GenericNotASaga<object>).IsSubclassOfSaga());
            Assert.IsFalse(typeof(InheritsGenericNotASaga<object>).IsSubclassOfSaga());
        }

        [TestMethod]
        public void IsSubClassOfSaga_WhenNull()
        {
            Type? type = null;
            Assert.IsFalse(type.IsSubclassOfSaga());
        }

    }
}
