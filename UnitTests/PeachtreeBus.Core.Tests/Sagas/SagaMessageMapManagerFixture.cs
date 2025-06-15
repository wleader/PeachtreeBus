using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Sagas;
using System.Diagnostics.CodeAnalysis;

namespace PeachtreeBus.Core.Tests.Sagas
{
    /// <summary>
    /// Proves the behavior of SagaMessageMapManager
    /// </summary>
    [TestClass]
    public class SagaMessageMapManagerFixture
    {
        private class MockMessage1 : IQueueMessage
        {
            public SagaKey Key { get; set; }
        }

        private class MockMessage2 : IQueueMessage
        {
            public SagaKey Key { get; set; }
        }

        private class MockSagaData { }

        private class MockSaga : Saga<MockSagaData>
        {
            public int ConfigureMessageKeysCount = 0;

            [ExcludeFromCodeCoverage]
            public override SagaName SagaName { get; } = new("MockSaga");

            public override void ConfigureMessageKeys(ISagaMessageMap mapper)
            {
                ConfigureMessageKeysCount++;
                mapper.Add<MockMessage1>((m) => m.Key);
                mapper.Add<MockMessage2>((m) => m.Key);
            }
        }

        private SagaMessageMapManager manager = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            manager = new SagaMessageMapManager();
        }

        /// <summary>
        /// Proves that the saga is configured only once.
        /// </summary>
        [TestMethod]
        public void GetKey_ConfiguresOnlyOnce_AndUsesProvidedFunctions()
        {
            var saga = new MockSaga();
            SagaKey FooKey = new("FooKey");
            SagaKey BazKey = new("BazKey");
            var message1 = new MockMessage1() { Key = FooKey };
            var message2 = new MockMessage2() { Key = BazKey };

            Assert.AreEqual(0, saga.ConfigureMessageKeysCount);

            var result1 = manager.GetKey(saga, message1);
            var result2 = manager.GetKey(saga, message2);

            Assert.AreEqual(FooKey, result1);
            Assert.AreEqual(BazKey, result2);
            Assert.AreEqual(1, saga.ConfigureMessageKeysCount);
        }
    }
}
