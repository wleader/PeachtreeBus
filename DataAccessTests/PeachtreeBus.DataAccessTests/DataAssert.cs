using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;

namespace PeachtreeBus.DataAccessTests;

public static class DataAssert
{
    public static void AreEqual(QueueData? expected, QueueData? actual)
    {
        Assert.IsFalse(expected is null && actual is null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        AreEqual(expected.Headers, actual.Headers);
        Assert.AreEqual(expected.MessageId, actual.MessageId);
        AreEqual(expected.NotBefore, actual.NotBefore);
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Body, actual.Body);
        AreEqual(expected.Completed, actual.Completed);
        AreEqual(expected.Enqueued, actual.Enqueued);
        AreEqual(expected.Failed, actual.Failed);
        Assert.AreEqual(expected.Retries, actual.Retries);
    }
    
    public static void AreEqual(Headers? expected, Headers? actual)
    {
        Assert.IsFalse(expected == null && actual == null, "Do not assert Null is Null.");
        Assert.IsNotNull(actual, "Actual is null, expected is not.");
        Assert.IsNotNull(expected, "Expected is null, actual is not.");
        Assert.AreEqual(expected.MessageClass, actual.MessageClass);
        Assert.AreEqual(expected.ExceptionDetails, actual.ExceptionDetails);
        CollectionAssert.AreEqual(expected.UserHeaders, actual.UserHeaders);
        Assert.AreEqual(expected.Diagnostics, actual.Diagnostics);
    }
    
    /// <summary>
    /// Tests that two nullable DateTime values are equal.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <param name="allowDriftMs">Allows a minor difference in times.</param>
    public static void AreEqual(DateTime? expected, DateTime? actual, int allowDriftMs = 100)
    {
        // if they are both null, it's ok.
        if (!expected.HasValue && !actual.HasValue) return;

        // if one is null and the other is not it's a failure.
        Assert.AreEqual(expected.HasValue, actual.HasValue, $"Expected {expected}, Actual {actual}");

        // both are not null, so compare deeper.
        AreEqual(expected!.Value, actual!.Value, allowDriftMs);
    }
    
    /// <summary>
    /// Tests that two DateTime values are equal.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <param name="allowDriftMs">Allows for a minor difference in times.</param>
    public static void AreEqual(DateTime expected, DateTime actual, int allowDriftMs = 500)
    {
        Assert.AreEqual(expected.Kind, actual.Kind);

        // date times the get stored in SQL, and because of the way things are stored
        // they can be off by a few ms, so just make sure it's close
        var actualDrift = Math.Abs(expected.Subtract(actual).TotalMilliseconds);
        Assert.IsLessThan(allowDriftMs, actualDrift);
    }
}