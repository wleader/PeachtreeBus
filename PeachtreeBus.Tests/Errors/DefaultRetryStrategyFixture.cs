using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Errors;
using PeachtreeBus.Queues;
using PeachtreeBus.Subscriptions;
using System;

namespace PeachtreeBus.Tests.Errors;


public abstract class DefaultRetryStrategyFixture<TContext, TStrategy>
    where TContext : IContext
    where TStrategy : DefaultRetryStrategy<TContext>, new()
{
    protected TContext Context = default!;
    protected TStrategy TestSubject = default!;

    private static readonly ApplicationException exception = new();

    [TestInitialize]
    public void Intialize()
    {
        Context = CreateContext();
        TestSubject = new();
    }
    protected abstract TContext CreateContext();

    /// <summary>
    /// Interesting note, a negative would result in a not-before in the past
    /// making for an immediate retry. Anyway its just supposed to be a basic
    /// retry system. Users that need something more sophiticated could make their own.
    /// </summary>
    [TestMethod]
    [DataRow(5, -1, true, -5)]
    [DataRow(5, 0, true, 0)]
    [DataRow(5, 1, true, 5)]
    [DataRow(5, 2, true, 10)]
    [DataRow(5, 3, true, 15)]
    [DataRow(5, 4, true, 20)]
    [DataRow(5, 5, false, 25)]
    [DataRow(5, 6, false, 30)]
    [DataRow(6, 5, true, 25)]
    public void Given_FailureCount_When_Determine_Then_RetryAndDelay(
        int maxRetries, int failureCount, bool expectRetry, int delaySeconds)
    {
        TestSubject.MaxRetries = maxRetries;
        Assert.AreEqual(maxRetries, TestSubject.MaxRetries);

        var result = TestSubject.DetermineRetry(Context, exception, failureCount);
        Assert.AreEqual(expectRetry, result.ShouldRetry);
        Assert.AreEqual(delaySeconds, result.Delay.TotalSeconds);
    }
}

[TestClass]
public class DefaultQueueRetryStrategyFixture
    : DefaultRetryStrategyFixture<IQueueContext, DefaultQueueRetryStrategy>
{
    protected override IQueueContext CreateContext() => TestData.CreateQueueContext();
}

[TestClass]
public class DefaultSubscribedRetryStrategyFixture
    : DefaultRetryStrategyFixture<ISubscribedContext, DefaultSubscribedRetryStrategy>
{
    protected override ISubscribedContext CreateContext() => TestData.CreateSubscribedContext();
}
