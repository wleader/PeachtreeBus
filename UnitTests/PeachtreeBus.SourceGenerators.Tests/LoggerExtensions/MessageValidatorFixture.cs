using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class MessageValidatorFixture
{
    [TestMethod]
    [DataRow(" ", DisplayName = "Whitespace1")]
    [DataRow("\r\n", DisplayName = "Whitespace2")]
    [DataRow("\t", DisplayName = "Whitespace3")]
    [DataRow((string)null!, DisplayName = "Null")]
    [DataRow("Hello\"World", DisplayName = "UnescapedQuotes")]
    [DataRow("Hello {Name}", DisplayName = "MissingType")]
    public void Given_InvalidMessage_When_Validate_Then_Throws(string value)
    {
        var validator = new MessageValidator();
        Assert.ThrowsExactly<ApplicationException>(() => validator.Validate(value));
    }

    [TestMethod]
    [DataRow("Hello \\\"World\\\"", DisplayName = "EscapedQuotes")]
    [DataRow("Hello World", DisplayName = "NoParameters")]
    [DataRow("Hello {Name:string}", DisplayName = "1Parameter")]
    [DataRow("Hello {Name1:string} and {Name2:string}", DisplayName = "2Parameters")]
    public void Given_ValidMessage_When_Validate_Then_Returns(string value)
    {
        var validator = new MessageValidator();
        validator.Validate(value);
    }
}
