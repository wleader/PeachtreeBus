using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;

namespace PeachtreeBus.Tests
{
    /// <summary>
    /// Proves the behavior of PerfCounters
    /// </summary>
    [TestClass]
    public class PerfCountersFixture
    {
        /// <summary>
        /// Holds data read from the performance counters system
        /// </summary>
        internal class EventData
        {
            public string Name;
            public int Count;
            public double Min;
            public double Max;
            public double Mean;
            public double Increment;
            public string Series;

            //[1]: "DisplayName"
            //[3]: "StandardDeviation"
            //[7]: "IntervalSec"
            //[8]: "Series"
            //[9]: "CounterType"
            //[10]: "Metadata"
            //[11]: "DisplayUnits"

            public EventData(object payload)
            {
                var d = (IDictionary<string, object>)payload;
                System.Diagnostics.Debug.Assert(d is not null);
                Name = d["Name"]?.ToString() ?? "NULL";
                Count = (int)(Get(d, "Count") ?? 0);
                Min = (double)(Get(d, "Min") ?? 0.0);
                Max = (double)(Get(d, "Max") ?? 0.0);
                Mean = (double)(Get(d, "Mean") ?? 0.0);
                Series = (string)(Get(d, "Series") ?? "NULL");
                Increment = (double)(Get(d, "Increment") ?? 0.0);
            }

            private static object? Get(IDictionary<string, object> d, string name)
            {
                d.TryGetValue(name, out var result);
                return result;
            }
        }

        /// <summary>
        /// recieves event data from teh performance counters system
        /// </summary>
        internal class PeachtreeBusEventListener : EventListener
        {
            private bool enabled;
            private string? ListenFor;
            private EventData? LastData;

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                base.OnEventWritten(eventData);
                if (!enabled) return;
                if (eventData.EventName != "EventCounters") return;
                var payload = eventData.Payload!.First();
                var data = new EventData(payload!);
                if (data.Name != ListenFor) return;
                LastData = data;
            }

            public EventData WaitOne(string name)
            {
                enabled = true;
                ListenFor = name;
                LastData = null;
                while (LastData == null)
                {
                    Thread.Yield();
                    Thread.Sleep(10);
                }
                enabled = false;
                return LastData;
            }

        }

        private PeachtreeBusEventListener listener = default!;

        [TestInitialize]
        public void TestInitialize()
        {
            var arguments = new Dictionary<string, string?>
            {
                {"EventCounterIntervalSec", ".1"}
            };

            listener = new PeachtreeBusEventListener();
            listener.EnableEvents(
                PerfCounters.Instance(),
                EventLevel.LogAlways,
                EventKeywords.All,
                arguments);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            listener.Dispose();
            listener = null!;
        }

        /// <summary>
        /// Proves the counters are setup at startup.
        /// </summary>
        [TestMethod]
        public void PerfCounters_IsSetupCorrectly()
        {
            var instance = PerfCounters.Instance();

            Assert.AreEqual("PeachtreeBus", instance.Name);
            Assert.IsNull(instance.ConstructionException);
        }

        /// <summary>
        /// Proves StartMessage and FinishMessage behave.
        /// </summary>
        [TestMethod]
        public void PerfCounters_MessagesActive_IncrementsAndDecrements()
        {
            var instance = PerfCounters.Instance();
            listener.WaitOne("messages-active");

            instance.StartMessage();
            var data = listener.WaitOne("messages-active");
            Assert.AreEqual(1, data.Min);
            Assert.AreEqual(1, data.Max);
            Assert.AreEqual(1, data.Mean);

            instance.StartMessage();
            data = listener.WaitOne("messages-active");
            Assert.AreEqual(2, data.Min);
            Assert.AreEqual(2, data.Max);
            Assert.AreEqual(2, data.Mean);

            instance.FinishMessage(DateTime.Now);
            data = listener.WaitOne("messages-active");
            Assert.AreEqual(1, data.Min);
            Assert.AreEqual(1, data.Max);
            Assert.AreEqual(1, data.Mean);

            instance.FinishMessage(DateTime.Now);
            data = listener.WaitOne("messages-active");
            Assert.AreEqual(0, data.Min);
            Assert.AreEqual(0, data.Max);
            Assert.AreEqual(0, data.Mean);
        }

        /// <summary>
        /// Proves messages attempted are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_FinishMessage_CountsAttempts()
        {
            AssertCounts(p => { p.StartMessage(); p.FinishMessage(DateTime.Now); }, "messages-attempted");
        }

        /// <summary>
        /// Proves that message time is counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_FinishMessage_MeasuresTime()
        {
            var instance = PerfCounters.Instance();

            instance.StartMessage();
            instance.StartMessage();
            instance.StartMessage();

            instance.FinishMessage(DateTime.Now.Subtract(new TimeSpan(TimeSpan.TicksPerSecond * 1)));
            instance.FinishMessage(DateTime.Now.Subtract(new TimeSpan(TimeSpan.TicksPerSecond * 2)));
            instance.FinishMessage(DateTime.Now.Subtract(new TimeSpan(TimeSpan.TicksPerSecond * 3)));

            var data = listener.WaitOne("message-time");
            // there should be three data points.
            Assert.AreEqual(3, data.Count);

            // the minumum should be about 1 second (10ms skew ok)
            AssertRange(1000, 10, data.Min, "data.Min");

            // the maximum should be about 3 second (10ms skew ok)
            AssertRange(3000, 10, data.Max, "data.Max");

            // the mean should be about 2 second (10ms skew ok)
            //(1+2+3)/3 = 2 seconds mean.
            AssertRange(2000, 10, data.Mean, "data.Mean");
        }

        /// <summary>
        /// Proves completed messages are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_CompleteMessage_Counts()
        {
            AssertCounts(p => p.CompleteMessage(), "messages-complete");
        }

        /// <summary>
        /// Proves sent messages are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_SentMessage_Counts()
        {
            AssertCounts(p => p.SentMessage(), "messages-sent");
        }

        /// <summary>
        /// Proves that delayed messages are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_DelayMessage_Counts()
        {
            AssertCounts(p => p.DelayMessage(), "messages-delayed");
        }

        /// <summary>
        /// proves error messages are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_ErrorMessage_Counts()
        {
            AssertCounts(p => p.FailMessage(), "messages-failed");
        }

        /// <summary>
        /// Proves retried messages are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_RetryMessage_Counts()
        {
            AssertCounts(p => p.RetryMessage(), "messages-retry");
        }

        /// <summary>
        /// proves blocked sagas are counted.
        /// </summary>
        [TestMethod]
        public void PerfCounters_SagaBlocked_Counts()
        {
            AssertCounts(p => p.SagaBlocked(), "saga-blocks");
        }

        /// <summary>
        /// Prove error messages are totaled.
        /// </summary>
        [TestMethod]
        public void PerfCounters_ErrorMessage_Totals()
        {
            PerfCounters.Instance().Reset();
            AssertCounts(p => p.FailMessage(), "messages-failed");

            var data = listener.WaitOne("messages-failed-total");
            Assert.AreEqual(8, data.Min);
            Assert.AreEqual(8, data.Max);
            Assert.AreEqual(8, data.Mean);
        }

        private void AssertCounts(Action<IPerfCounters> action, string name)
        {
            AssertCount(action, name, 1);
            AssertCount(action, name, 2);
            AssertCount(action, name, 5);
        }

        private void AssertCount(Action<IPerfCounters> action, string name, int count)
        {
            var instance = PerfCounters.Instance();
            var data = listener.WaitOne(name);
            Assert.AreEqual(0, data.Increment);
            for (var i = 0; i < count; i++)
            {
                action.Invoke(instance);
            }
            data = listener.WaitOne(name);
            Assert.AreEqual(count, data.Increment);
        }

        public static void AssertRange(double expected, double tolerance, double actual, string valueName)
        {
            var max = expected + tolerance;
            var min = expected - tolerance;
            Assert.IsTrue(actual >= min && actual <= max, $"The value {actual} for {valueName} was outside the range {min}(min) - {max}(max).");
        }
    }
}
