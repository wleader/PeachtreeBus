using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using PeachtreeBus.Subscriptions;

namespace PeachtreeBus.Tests.Sagas;

[TestClass]
public class TopicFixture
{
    [TestMethod]
    [DataRow((string)null!)]
    [DataRow("")]
    public void Given_String_When_New_Then_Throws(string value)
    {
        Assert.ThrowsException<TopicException>(() => _ = new Topic(value));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(SagaKey.MaxLength + 1)]
    public void Given_StringOfLength_When_New_Then_Throws(int length)
    {
        Assert.ThrowsException<TopicException>(() =>
            _ = new Topic(new string('x', length)));
    }

    [TestMethod]
    public void Given_StringOfMaxLength_When_New_Then_Result()
    {
        var expected = new string('U', Topic.MaxLength);
        var actual = new Topic(expected);
        Assert.AreEqual(expected, actual.Value);
    }

    [TestMethod]
    public void Given_Topic_When_ToString_Then_Result()
    {
        var expected = "FooBar";
        var actual = new Topic(expected);
        Assert.AreEqual(expected, actual.ToString());
        Assert.AreEqual(expected, actual.Value);
    }

    [TestMethod]
    public void Given_Uninitialized_When_ToString_Then_Throws()
    {
        Assert.ThrowsException<TopicException>(() =>
        _ = ((Topic)default).ToString());
    }

    [TestMethod]
    public void Given_Uninitialized_When_GetValue_Then_Throws()
    {
        Assert.ThrowsException<TopicException>(() =>
        _ = ((Topic)default).Value);
    }
}
