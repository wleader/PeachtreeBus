using Moq;
using PeachtreeBus.Data;
using System;
using System.Collections.Generic;

namespace PeachtreeBus.Tests.Fakes;

public class FakeSerializer
{
    public readonly record struct SerializeParameters(object Object, Type Type);

    public Mock<ISerializer> Mock { get; init; }
    public ISerializer Object { get => Mock.Object; }

    public SerializedData SerializeHeadersResult { get; set; } = TestData.DefaultHeaders;
    public SerializedData SerializeMessageResult { get; set; } = TestData.DefaultBody;
    public SerializedData SerializeSagaResult { get; set; } = TestData.DefaultSagaData;
    public List<Headers> SerializedHeaders { get; } = [];
    public List<SerializeParameters> SerializedMessages { get; } = [];
    public List<SerializeParameters> SerializedSagas { get; } = [];

    public FakeSerializer()
    {
        Mock = new();

        Mock.Setup(s => s.SerializeHeaders(It.IsAny<Headers>()))
            .Callback((Headers h) => SerializedHeaders.Add(h))
            .Returns(() => SerializeHeadersResult);

        Mock.Setup(s => s.SerializeMessage(It.IsAny<object>(), It.IsAny<Type>()))
            .Callback((object o, Type t) => SerializedMessages.Add(new(o, t)))
            .Returns(() => SerializeMessageResult);

        Mock.Setup(s => s.SerializeSaga(It.IsAny<object>(), It.IsAny<Type>()))
            .Callback((object o, Type t) => SerializedSagas.Add(new(o, t)))
            .Returns(() => SerializeSagaResult);
    }

}
