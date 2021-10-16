using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peachtreebus.Tests
{
    [TestClass]
    public class SagaMessageMapFixture
    {
        [TestMethod]
        public void CorrectFunctinIsInvokedForEachMessageType()
        {
            int invokeCount1 = 0;
            int invokeCount2 = 0;

            var map = new SagaMessageMap();
            map.Add<TestSagaMessage1>((m) => { invokeCount1++; return "Function1"; });
            map.Add<TestSagaMessage2>((m) => { invokeCount2++; return "Function2";  });

            Assert.AreEqual("Function1", map.GetKey(new TestSagaMessage1()));
            Assert.AreEqual("Function2", map.GetKey(new TestSagaMessage2()));
            
            Assert.AreEqual(1, invokeCount1);
            Assert.AreEqual(1, invokeCount2);

            Assert.AreEqual("Function1", map.GetKey(new TestSagaMessage1()));
            Assert.AreEqual("Function2", map.GetKey(new TestSagaMessage2()));

            Assert.AreEqual(2, invokeCount1);
            Assert.AreEqual(2, invokeCount2);
        }

        [TestMethod]
        [ExpectedException(typeof(SagaMapException))]
        public void TheSameMessageCanNotBeAddedTwice()
        {
            var map = new SagaMessageMap();
            map.Add<TestSagaMessage1>((m) => "Function1");
            map.Add<TestSagaMessage1>((m) => "Function2");
        }

        [TestMethod]
        [ExpectedException(typeof(SagaMapException))]
        public void GetKey_ThrowsForUnmappedMessageType()
        {
            var map = new SagaMessageMap();
            map.GetKey(new TestSagaMessage1());
        }
    }
}
