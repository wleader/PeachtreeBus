using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Sagas;
using PeachtreeBus.Serialization;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.Core.Tests.Sagas;

[TestClass]
public class SagaMetaDataSerializationFixture
{
    public static IEnumerable<object[]> TestData =>
    [
        [
            """{"Started":"2025-07-23T12:28:04.3425783Z","LastMessageTime":"2025-07-23T12:28:04.3425783Z"}""",
            "2025-07-23T12:28:04.3425783Z",
            "2025-07-23T12:28:04.3425783Z",
            """{"Started":"2025-07-23T12:28:04.3425783Z","LastMessageTime":"2025-07-23T12:28:04.3425783Z"}""",
        ],
        [
            "{}",
            "0001-01-01T00:00:00.0000000Z",
            "0001-01-01T00:00:00.0000000Z",
            """{"Started":"0001-01-01T00:00:00Z","LastMessageTime":"0001-01-01T00:00:00Z"}""",
        ],
        [
            """{"Started":"0001-01-01T00:00:00Z","LastMessageTime":"0001-01-01T00:00:00Z"}""",
            "0001-01-01T00:00:00.0000000Z",
            "0001-01-01T00:00:00.0000000Z",
            """{"Started":"0001-01-01T00:00:00Z","LastMessageTime":"0001-01-01T00:00:00Z"}""",
        ],
        [
            """{"Started":"2025-05-24T19:39:43.6156895Z","LastMessageTime":"2025-05-24T19:39:43.6156909Z"}""",
            "2025-05-24T19:39:43.6156895Z",
            "2025-05-24T19:39:43.6156909Z",
            """{"Started":"2025-05-24T19:39:43.6156895Z","LastMessageTime":"2025-05-24T19:39:43.6156909Z"}""",
        ]
    ];

    private static DateTime ParseJsonDateTime(string value) =>
        System.Text.Json.JsonSerializer.Deserialize<DateTime>($"\"{value}\"");

    [TestMethod]
    [DynamicData(nameof(TestData))]
    public void Given_JsonAndTimes_When_RoundTrip_Then_Correct(string actualJson, string started, string lastMessageTime, string expectedJson)
    {
        var dtStarted = ParseJsonDateTime(started);
        var dtLastMessage = ParseJsonDateTime(lastMessageTime);

        var serializer = new DefaultSerializer();
        var metaData = serializer.Deserialize<SagaMetaData>(new(actualJson));

        Assert.AreEqual(dtStarted, metaData.Started.Value);
        Assert.AreEqual(dtLastMessage, metaData.LastMessageTime.Value);

        var serializedJson = serializer.Serialize(metaData);
        Assert.AreEqual(expectedJson, serializedJson.Value);
    }
}
