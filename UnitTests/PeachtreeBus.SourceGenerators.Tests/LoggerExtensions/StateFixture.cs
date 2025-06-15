using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.SourceGenerators.LoggerExtensions;
using System;

namespace PeachtreeBus.SourceGenerators.Tests.LoggerExtensions;

[TestClass]
public class StateFixture
{
    private State _state = default!;

    [TestInitialize]
    public void Initialize()
    {
        _state = new();
    }

    [TestMethod]
    public void When_New_Then_Defaults()
    {
        Assert.AreEqual(0, _state.AssemblyId);
        Assert.AreEqual(0, _state.NamespaceId);
        Assert.AreEqual(0, _state.ClassId);
        Assert.AreEqual(0, _state.EventId);
        Assert.IsFalse(_state.ExcludeFromCodeCoverage);
        Assert.AreEqual(string.Empty, _state.NamespaceUnderscored);
        Assert.AreEqual(string.Empty, _state.EventFullName);
        Assert.AreEqual(string.Empty, _state.CombinedId);
        Assert.AreEqual(string.Empty, _state.GenericConstraint);
        Assert.AreEqual(string.Empty, _state.ClassName);
        Assert.IsNull(_state.GenericArgs);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(13)]
    [DataRow(999)]
    public void Given_ValidId_When_Initialize_Then_IdIsSet(int expected)
    {
        AssemblyType data = new()
        {
            assemblyId = expected,
        };

        _state.Initialize(data);
        Assert.AreEqual(expected, _state.AssemblyId);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1000)]
    public void Given_InvalidId_When_Intialize_Then_Throws(int invalid)
    {
        AssemblyType data = new()
        {
            assemblyId = invalid
        };

        Assert.ThrowsExactly<ApplicationException>(() => _state.Initialize(data));
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, true)]
    public void Given_ExcludeSettings_When_Initialize_Then_ExcludeIsFalse(bool specified, bool value, bool expected)
    {
        AssemblyType data = new()
        {
            assemblyId = 1,
            exludeFromCodeCoverageSpecified = specified,
            exludeFromCodeCoverage = value,
        };

        _state.Initialize(data);
        Assert.AreEqual(expected, _state.ExcludeFromCodeCoverage);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(13)]
    [DataRow(99)]
    public void Given_ValidId_When_SetNamespace_Then_IdIsSet(int expected)
    {
        NamespaceType data = new()
        {
            namespaceId = expected,
        };

        _state.SetNamespace(data);
        Assert.AreEqual(expected, _state.NamespaceId);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(100)]
    public void Given_InvalidId_When_SetNamespace_Then_Throws(int invalid)
    {
        NamespaceType data = new()
        {
            namespaceId = invalid,
        };

        Assert.ThrowsExactly<ApplicationException>(() => _state.SetNamespace(data));
    }

    [TestMethod]
    [DataRow("name.space", "name_space")]
    [DataRow("name.space.one", "name_space_one")]
    [DataRow("name.space.one.two", "name_space_one_two")]
    [DataRow("PeachtreeBus.Data", "PeachtreeBus_Data")]
    public void Given_Name_When_SetNamespace_Then_UnderscoredIsSet(string name, string expected)
    {
        NamespaceType data = new()
        {
            namespaceId = 1,
            name = name,
        };

        _state.SetNamespace(data);
        Assert.AreEqual(expected, _state.NamespaceUnderscored);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(13)]
    [DataRow(99)]
    public void Given_ValidId_When_SetClass_Then_IdIsSet(int expected)
    {
        ClassType data = new()
        {
            classId = expected,
        };

        _state.SetClass(data);
        Assert.AreEqual(expected, _state.ClassId);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(100)]
    public void Given_InvalidId_When_SetClass_Then_Throws(int invalid)
    {
        ClassType data = new()
        {
            classId = invalid,
        };
        Assert.ThrowsExactly<ApplicationException>(() => _state.SetClass(data));
    }

    [TestMethod]
    [DataRow("TestClass")]
    [DataRow("TestClass2")]
    public void Given_Name_When_SetClass_Then_ClassName(string expected)
    {
        ClassType data = new()
        {
            classId = 1,
            name = expected
        };
        _state.SetClass(data);
        Assert.AreEqual(expected, _state.ClassName);
    }

    [TestMethod]
    [DataRow("string")]
    [DataRow("string, int")]
    public void Given_GenericArgs_When_SetClass_Then_GenericArgsAreSet(string value)
    {
        ClassType data = new()
        {
            classId = 1,
            genericArgs = value
        };
        _state.SetClass(data);
        var expected = "<" + value + ">";
        Assert.AreEqual(expected, _state.GenericArgs);
    }

    [TestMethod]
    public void Given_GenericArgsNull_When_SetClass_Then_GenericArgsNull()
    {
        ClassType data = new()
        {
            classId = 1,
            genericArgs = null
        };
        _state.SetClass(data);
        Assert.IsNull(_state.GenericArgs);
    }

    [TestMethod]
    [DataRow("where TBaseTask : IBaseTask")]
    [DataRow("where T : class")]
    public void Given_GenericConstraint_When_SetClass_Then_GenericConstraintSet(string value)
    {
        ClassType data = new()
        {
            classId = 1,
            genericConstraint = value
        };
        _state.SetClass(data);
        Assert.AreEqual(value, _state.GenericConstraint);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(13)]
    [DataRow(99)]
    public void Given_ValidId_When_SetEvent_Then_IdIsSet(int expected)
    {
        EventType data = new()
        {
            eventId = expected,
        };

        _state.SetEvent(data);
        Assert.AreEqual(expected, _state.EventId);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(100)]
    public void Given_InvalidId_When_SetEvent_Then_Throws(int invalid)
    {
        EventType data = new()
        {
            eventId = invalid,
        };
        Assert.ThrowsExactly<ApplicationException>(() => _state.SetEvent(data));
    }

    [TestMethod]
    [DataRow("Namespace", "Class", "Event", "Namespace_Class_Event")]
    [DataRow("Namespace.Sub", "Class", "Event", "Namespace_Sub_Class_Event")]
    public void Given_NamespaceClassEvent_When_SetEvent_Then_FullNameSet(string namespaceName, string className, string eventName, string expected)
    {
        NamespaceType namespaceType = new() { namespaceId = 1, name = namespaceName };
        _state.SetNamespace(namespaceType);
        ClassType classType = new() { classId = 1, name = className };
        _state.SetClass(classType);
        EventType eventType = new() { eventId = 1, name = eventName };
        _state.SetEvent(eventType);
        Assert.AreEqual(expected, _state.EventFullName);
    }

    [TestMethod]
    [DataRow(1, 1, 1, 1, "1010101")]
    [DataRow(999, 99, 99, 99, "999999999")]
    [DataRow(5, 10, 15, 20, "5101520")]
    public void Given_AssemblyNamespaceClassEvent_When_SetEvent_Then_CombinedIdSet(int assemblyId, int namespaceId, int classId, int eventId, string expected)
    {
        AssemblyType assemblyType = new() { assemblyId = assemblyId };
        _state.Initialize(assemblyType);
        NamespaceType namespaceType = new() { namespaceId = namespaceId, name = "Namespace.Sub" };
        _state.SetNamespace(namespaceType);
        ClassType classType = new() { classId = classId, name = "Class" };
        _state.SetClass(classType);
        EventType eventType = new() { eventId = eventId, name = "Event" };
        _state.SetEvent(eventType);
        Assert.AreEqual(expected, _state.CombinedId);
    }

}
