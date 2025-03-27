using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;

namespace PeachtreeBus.Tests.Telemetry;

[TestClass]
public class MetersFixture
{
    private Meters _meters = default!;
    private List<Instrument> _assertedUnchangedInstruments = [];

    [TestInitialize]
    public void TestInitialize()
    {
        _assertedUnchangedInstruments = [];
        _meters = new();
    }

    [TestMethod]
    public void Then_MeterVersionDoesNotNeedAChange()
    {
        // this is our meter name, no one else should use it.
        // it shouldn't change without a really good reason.
        Assert.AreEqual("PeachtreeBus.Messaging", Meters.Messaging.Name);

        // Version might change, if the counters inside the meter change.
        Assert.AreEqual("0.11.0", Meters.Messaging.Version);

        // get all the fields fields in the meter.
        var fields = typeof(Meters).GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.Static);

        // return true if the type inherits from Instrument.
        static bool IsInstrument(Type type) => 
            typeof(Instrument).IsAssignableFrom(type);

        var instrumentCount = fields
            .Select(f => f.FieldType)
            .Count(IsInstrument);

        const string ChangeVersionMessage =
            "If the instruments change, then the Meter.Version must change.";

        // if an instrument is added or removed, the version should change.
        Assert.AreEqual(7, instrumentCount, ChangeVersionMessage);

        // if an instrument name, unit, or type changes, the version should change.
        AssertInstrumentUnchanged(Meters.CompletedMessageCount,
            "messaging.client.consumed.messages", "messages", typeof(long), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.ActiveMessageCount,
            "peachtreebus.client.active.messages", "messages", typeof(int), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.AttemptedMessageCount,
            "peachtreebus.client.attempted.messages", "messages", typeof(long), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.FailedMessageCount,
            "peachtreebus.client.failed.messages", "messages", typeof(long), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.SentMessageCount,
            "messaging.client.sent.messages", "messages", typeof(long), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.RetryMessageCount,
            "peachtreebus.client.retry.messages", "messages", typeof(long), ChangeVersionMessage);
        AssertInstrumentUnchanged(Meters.BlockedSagaMessageCount,
            "peachtreebus.client.blockedsaga.messsages", "messages", typeof(long), ChangeVersionMessage);

        // if this fails, add or remove above as needed.
        Assert.AreEqual(instrumentCount, _assertedUnchangedInstruments.Distinct().Count(),
            "An Instrument was added or removed, but the test was not updated.");
    }

    private void AssertInstrumentUnchanged<T>(
        Instrument<T> instrument,
        string expectedName,
        string expectedUnit,
        Type expectedType,
        string? failMessage = null)
        where T: struct
    {
        _assertedUnchangedInstruments.Add(instrument);
        Assert.AreEqual(expectedName, instrument.Name, failMessage);
        Assert.AreEqual(expectedUnit, instrument.Unit, failMessage);
        Assert.AreEqual(expectedType, typeof(T), failMessage);
    }

    [TestMethod]
    public void When_CompleteMessage_Then_CompletedCountIncrements()
    {
        AssertMeasurement(Meters.CompletedMessageCount, _meters.CompleteMessage, 1);
    }

    [TestMethod]
    public void When_StartMessage_Then_AttemptedCountIncrements()
    {
        AssertMeasurement(Meters.AttemptedMessageCount, _meters.StartMessage, 1);
    }

    [TestMethod]
    public void When_StartMessage_Then_ActiveCountIncrements()
    {
        AssertMeasurement(Meters.ActiveMessageCount, _meters.StartMessage, 1);
    }

    [TestMethod]
    public void When_FinishMessage_Then_ActiveMessageCountDecrements()
    {
        AssertMeasurement(Meters.ActiveMessageCount, _meters.FinishMessage, -1);
    }

    [TestMethod]
    public void When_RetryMessage_Then_RetryCountIncrements()
    {
        AssertMeasurement(Meters.RetryMessageCount, _meters.RetryMessage, 1);
    }

    [TestMethod]
    public void When_FailMessage_Then_FailedMessageIncrements()
    { 
        AssertMeasurement(Meters.FailedMessageCount, _meters.FailMessage, 1);
    }

    [TestMethod]
    public void When_SagaBlocked_Then_LockedSagaIncrements()
    {
        AssertMeasurement(Meters.BlockedSagaMessageCount, _meters.SagaBlocked, 1);
    }

    [TestMethod]
    [DataRow(0, DisplayName = "0")]
    [DataRow(1, DisplayName = "1")]
    [DataRow(10, DisplayName = "10")]
    [DataRow(long.MaxValue, DisplayName = "long.MaxValue")]
    public void Given_Count_When_SentMessage_Then_Measured(long value)
    {
        AssertMeasurement(Meters.SentMessageCount,
            () => _meters.SentMessage(value), value);
    }

    [TestMethod]
    [DataRow(-1, DisplayName =  "-1")]
    [DataRow(long.MinValue, DisplayName = "long.MinValue")]
    public void Given_NegativeCount_When_SentMessage_Then_Throws(long value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => _meters.SentMessage(value));
    }

    [TestMethod]
    public void Given_LargeValues_When_SentMessage_Then_DoesNotThrow()
    {
        // Kinda cool that it doesn't throw an overflow.
        _meters.SentMessage(long.MaxValue);
        _meters.SentMessage(long.MaxValue);
    }

    private static void AssertMeasurement<T>(Instrument<T> instrument, Action action, T expected)
        where T : struct
    {
        var collector = new MetricCollector<T>(instrument);
        action();
        var measurements = collector.GetMeasurementSnapshot();
        Assert.AreEqual(1, measurements.Count);
        Assert.AreEqual(expected, measurements[0].Value);
    }
}
