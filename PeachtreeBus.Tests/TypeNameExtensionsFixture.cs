using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.Queues;
using PeachtreeBus.Tests.Sagas;
using System;

namespace PeachtreeBus.Tests;

[TestClass]
public class TypeNameExtensionsFixture
{
    [TestMethod]
    public void Given_SimpleObject_Then_NamesAreCorrect()
    {
        var value = new TestQueuedMessage();

        Assert.AreEqual(
            "PeachtreeBus.Abstractions.Tests.TestClasses.TestQueuedMessage",
            value.GetTypeFullName());
        Assert.AreEqual(
            "PeachtreeBus.Abstractions.Tests.TestClasses.TestQueuedMessage, PeachtreeBus.Abstractions.Tests",
            value.GetMessageClass());
    }

    [TestMethod]
    public void Given_GenericObject_Then_NamesAreCorrect()
    {
        var value = new Mock<IQueueMessage>();

        Assert.AreEqual(
            "Moq.Mock`1[[PeachtreeBus.Queues.IQueueMessage, PeachtreeBus.Abstractions]]",
            value.GetTypeFullName());
        Assert.AreEqual(
            "Moq.Mock`1[[PeachtreeBus.Queues.IQueueMessage, PeachtreeBus.Abstractions]], Moq",
            value.GetMessageClass());
    }

    [TestMethod]
    public void Given_Tuple_Then_NamesAreCorrect()
    {
        var value = new Tuple<string, int>("", 0);

        Assert.AreEqual(
            "System.Tuple`2[[System.String, System.Private.CoreLib], [System.Int32, System.Private.CoreLib]]",
            value.GetTypeFullName());
        Assert.AreEqual(
            "System.Tuple`2[[System.String, System.Private.CoreLib], [System.Int32, System.Private.CoreLib]], System.Private.CoreLib",
            value.GetMessageClass());
    }

    [TestMethod]
    public void Given_UnboundGeneric_Then_NamesAreCorrect()
    {
        var type = typeof(Tuple<,>);
        Assert.AreEqual(
            "System.Tuple`2", type.GetTypeFullName());
        Assert.AreEqual(
            "System.Tuple`2, System.Private.CoreLib", type.GetMessageClass());
    }

    private class Nested
    {
        public class Nested2<T>;
    };

    [TestMethod]
    public void Given_NestedClass_Then_NamesAreCorrect()
    {
        var type = typeof(Nested);
        Assert.AreEqual(
            "PeachtreeBus.Tests.TypeNameExtensionsFixture+Nested", type.GetTypeFullName());
        Assert.AreEqual(
            "PeachtreeBus.Tests.TypeNameExtensionsFixture+Nested, PeachtreeBus.Tests", type.GetMessageClass());
    }

    [TestMethod]
    public void Given_DoubleNestedClass_Then_NamesAreCorrect()
    {
        var type = typeof(Nested.Nested2<int>);
        Assert.AreEqual(
            "PeachtreeBus.Tests.TypeNameExtensionsFixture+Nested+Nested2`1[[System.Int32, System.Private.CoreLib]]", type.GetTypeFullName());
        Assert.AreEqual(
            "PeachtreeBus.Tests.TypeNameExtensionsFixture+Nested+Nested2`1[[System.Int32, System.Private.CoreLib]], PeachtreeBus.Tests", type.GetMessageClass());
    }


    [TestMethod]
    public void Given_Objects_When_GetMessageClass_Then_TypeRoundTrips()
    {
        // a normal message
        AssertMessageClassRoundTrip(typeof(TestQueuedMessage));
        AssertMessageClassRoundTrip(typeof(TestSaga));
        AssertMessageClassRoundTrip(typeof(Mock<IQueueMessage>));
        AssertMessageClassRoundTrip(typeof(Tuple<string, int>));
        AssertMessageClassRoundTrip(typeof(Tuple<,>));
    }

    private static void AssertMessageClassRoundTrip(Type type)
    {
        var classString = type.GetMessageClass();
        Assert.IsNotNull(classString, $"Could not get MessageClass for {type}");
        var reconstructedType = Type.GetType(classString);
        Assert.IsNotNull(reconstructedType, $"Could not get type from string {classString}");
        Assert.AreEqual(type, reconstructedType, "Recreated type did not match original type.");
    }
}
