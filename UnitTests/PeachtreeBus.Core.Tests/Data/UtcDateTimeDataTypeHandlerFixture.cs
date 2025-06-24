using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;
using System;
using System.Data;

namespace PeachtreeBus.Core.Tests.Data;

[TestClass]
public class UtcDateTimeDataTypeHandlerFixture : TypeHandlerFixtureBase<UtcDateTime, UtcDateTimeHandler>
{
    private DateTime StringToUnspecified(string value) => DateTime.Parse(value);
    private DateTime StringToUtc(string value) => DateTime.SpecifyKind(DateTime.Parse(value), DateTimeKind.Utc);

    [TestMethod]
    [DataRow("2025-06-20 21:37:38.4778237")]
    public void Given_Value_When_SetValue_Then_ParameterIsSetup(string value)
    {
        var dateTime = StringToUtc(value);
        VerifySetValue(dateTime, DbType.DateTime2, dateTime);
    }

    [TestMethod]
    [DataRow("2025-06-20 21:37:38.4778237")]
    public void Given_Value_When_Parse_Then_Result(string value)
    {
        VerifyParse(StringToUnspecified(value), StringToUtc(value));
    }
}
