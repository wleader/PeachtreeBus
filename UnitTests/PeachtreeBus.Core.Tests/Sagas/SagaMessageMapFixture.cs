using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Core.Tests.Sagas
{
    /// <summary>
    /// Proves the behavior of SagaMessageMap
    /// </summary>
    [TestClass]
    public class SagaMessageMapFixture
    {
        private readonly SagaKey _sagaKey1 = new("SagaKey1");
        private readonly SagaKey _sagaKey2 = new("SagaKey2");

        [TestMethod]
        public void CorrectFunctionIsInvokedForEachMessageType()
        {
            int invokeCount1 = 0;
            int invokeCount2 = 0;

            var map = new SagaMessageMap();
            map.Add<TestSagaMessage1>(_ => { invokeCount1++; return _sagaKey1; });
            map.Add<TestSagaMessage2>(_ => { invokeCount2++; return _sagaKey2; });

            Assert.AreEqual(_sagaKey1, map.GetKey(new TestSagaMessage1()));
            Assert.AreEqual(_sagaKey2, map.GetKey(new TestSagaMessage2()));

            Assert.AreEqual(1, invokeCount1);
            Assert.AreEqual(1, invokeCount2);

            Assert.AreEqual(_sagaKey1, map.GetKey(new TestSagaMessage1()));
            Assert.AreEqual(_sagaKey2, map.GetKey(new TestSagaMessage2()));

            Assert.AreEqual(2, invokeCount1);
            Assert.AreEqual(2, invokeCount2);
        }

        [ExcludeFromCodeCoverage]
        private SagaKey GetKey1(TestSagaMessage1 m) { return _sagaKey1; }

        [ExcludeFromCodeCoverage]
        private SagaKey GetKey2(TestSagaMessage1 m) { return _sagaKey2; }

        /// <summary>
        /// Proves that the same message cannot be added twice.
        /// </summary>
        [TestMethod]
        public void TheSameMessageCanNotBeAddedTwice()
        {
            var map = new SagaMessageMap();
            map.Add<TestSagaMessage1>(GetKey1);
            Assert.ThrowsExactly<SagaMapException>(() =>
                map.Add<TestSagaMessage1>(GetKey2));
        }

        /// <summary>
        /// Proves exception is thrown for unmapped message type.
        /// </summary>
        [TestMethod]
        public void GetKey_ThrowsForUnmappedMessageType()
        {
            var map = new SagaMessageMap();
            Assert.ThrowsExactly<SagaMapException>(() =>
                map.GetKey(new TestSagaMessage1()));
        }
    }
}
