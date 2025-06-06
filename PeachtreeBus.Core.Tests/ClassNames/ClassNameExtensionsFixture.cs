﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeachtreeBus.Abstractions.Tests.TestClasses;
using PeachtreeBus.ClassNames;
using PeachtreeBus.Core.Tests.Sagas;
using PeachtreeBus.Queues;
using System;

namespace PeachtreeBus.Core.Tests.ClassNames;

[TestClass]
public class ClassNameExtensionsFixture
{
    [TestMethod]
    public void Given_SimpleObject_Then_NamesAreCorrect()
    {
        var value = new TestQueuedMessage();

        Assert.AreEqual(
            "PeachtreeBus.Abstractions.Tests.TestClasses.TestQueuedMessage",
            value.GetTypeFullName());
        Assert.AreEqual(
            new("PeachtreeBus.Abstractions.Tests.TestClasses.TestQueuedMessage, PeachtreeBus.Abstractions.Tests"),
            value.GetClassName());
    }

    [TestMethod]
    public void Given_GenericObject_Then_NamesAreCorrect()
    {
        var value = new Mock<IQueueMessage>();

        Assert.AreEqual(
            "Moq.Mock`1[[PeachtreeBus.Queues.IQueueMessage, PeachtreeBus.MessageInterfaces]]",
            value.GetTypeFullName());
        Assert.AreEqual(
            new("Moq.Mock`1[[PeachtreeBus.Queues.IQueueMessage, PeachtreeBus.MessageInterfaces]], Moq"),
            value.GetClassName());
    }

    [TestMethod]
    public void Given_Tuple_Then_NamesAreCorrect()
    {
        var value = new Tuple<string, int>("", 0);

        Assert.AreEqual(
            "System.Tuple`2[[System.String, System.Private.CoreLib], [System.Int32, System.Private.CoreLib]]",
            value.GetTypeFullName());
        Assert.AreEqual(
            new("System.Tuple`2[[System.String, System.Private.CoreLib], [System.Int32, System.Private.CoreLib]], System.Private.CoreLib"),
            value.GetClassName());
    }

    [TestMethod]
    public void Given_UnboundGeneric_Then_NamesAreCorrect()
    {
        var type = typeof(Tuple<,>);
        Assert.AreEqual(
            "System.Tuple`2", type.GetTypeFullName());
        Assert.AreEqual(
            new("System.Tuple`2, System.Private.CoreLib"), type.GetClassName());
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
            "PeachtreeBus.Core.Tests.ClassNames.ClassNameExtensionsFixture+Nested", type.GetTypeFullName());
        Assert.AreEqual(
            new("PeachtreeBus.Core.Tests.ClassNames.ClassNameExtensionsFixture+Nested, PeachtreeBus.Core.Tests"), type.GetClassName());
    }

    [TestMethod]
    public void Given_DoubleNestedClass_Then_NamesAreCorrect()
    {
        var type = typeof(Nested.Nested2<int>);
        Assert.AreEqual(
            "PeachtreeBus.Core.Tests.ClassNames.ClassNameExtensionsFixture+Nested+Nested2`1[[System.Int32, System.Private.CoreLib]]", type.GetTypeFullName());
        Assert.AreEqual(
            new("PeachtreeBus.Core.Tests.ClassNames.ClassNameExtensionsFixture+Nested+Nested2`1[[System.Int32, System.Private.CoreLib]], PeachtreeBus.Core.Tests"), type.GetClassName());
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
        var className = type.GetClassName();
        var reconstructedType = Type.GetType(className.Value);
        Assert.IsNotNull(reconstructedType, $"Could not get type from string {className}");
        Assert.AreEqual(type, reconstructedType, "Recreated type did not match original type.");
    }
}
