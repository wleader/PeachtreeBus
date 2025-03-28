using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Queues;
using PeachtreeBus.Serialization;
using System;

namespace PeachtreeBus.Tests.Serialization;

[TestClass]
public class DefaultSerializerFixture
{
    private DefaultSerializer serializer = default!;

    [TestInitialize]
    public void Intialize()
    {
        serializer = new();
    }

    [TestMethod]
    public void Given_Headers_When_Roundtrip_Then_DataIsCorrect()
    {
        var headers = new Headers()
        {
            ExceptionDetails = "ExceptionDetails",
            MessageClass = "MessageClass",
        };
        headers.UserHeaders.Add("UserHeader.One", "One");
        headers.UserHeaders.Add("UserHeader.Two", "Two");
        headers.Diagnostics.TraceParent = Guid.NewGuid().ToString();
        headers.Diagnostics.StartNewTraceOnReceive = true;

        var serialized = serializer.SerializeHeaders(headers);
        var deserialized = serializer.DeserializeHeaders(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(headers.ExceptionDetails, deserialized.ExceptionDetails);
        Assert.AreEqual(headers.MessageClass, deserialized.MessageClass);
        Assert.AreEqual(headers.UserHeaders.Count, deserialized.UserHeaders.Count);
        Assert.IsTrue(headers.UserHeaders.ContainsKey("UserHeader.One"));
        Assert.AreEqual("One", deserialized.UserHeaders["UserHeader.One"]);
        Assert.IsTrue(headers.UserHeaders.ContainsKey("UserHeader.Two"));
        Assert.AreEqual("Two", deserialized.UserHeaders["UserHeader.Two"]);
        Assert.AreEqual(headers.Diagnostics.TraceParent, deserialized.Diagnostics.TraceParent);
        Assert.AreEqual(headers.Diagnostics.StartNewTraceOnReceive, deserialized.Diagnostics.StartNewTraceOnReceive);
    }

    public class UserMessage : IQueueMessage
    {
        public string Foo { get; set; } = string.Empty;
        public int Bar { get; set; }
    }

    [TestMethod]
    public void Given_UserMessage_When_Roundtrip_Then_DataIsCorrect()
    {
        var message = new UserMessage() { Foo = "Baz", Bar = 42 };

        var serialized = serializer.SerializeMessage(message, typeof(UserMessage));
        var deserialized = (UserMessage)serializer.DeserializeMessage(serialized, typeof(UserMessage));

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(message.Foo, deserialized.Foo);
        Assert.AreEqual(message.Bar, deserialized.Bar);
    }

    public class UserSaga
    {
        public string Foo { get; set; } = string.Empty;
        public int Bar { get; set; }
    }

    [TestMethod]
    public void Given_UserSaga_When_Roundtrip_Then_DataIsCorrect()
    {
        var message = new UserSaga() { Foo = "Baz", Bar = 42 };

        var serialized = serializer.SerializeSaga(message, typeof(UserSaga));
        var deserialized = (UserSaga)serializer.DeserializeSaga(serialized, typeof(UserSaga));

        Assert.IsNotNull(deserialized);
        Assert.AreEqual(message.Foo, deserialized.Foo);
        Assert.AreEqual(message.Bar, deserialized.Bar);
    }
}
