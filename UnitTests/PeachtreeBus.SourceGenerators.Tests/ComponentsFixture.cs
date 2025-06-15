using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.SourceGenerators.Tests;

[TestClass]
public class ComponentsFixture
{
    [TestMethod]
    public void When_VerifyContainer_Then_DoesNotThrow()
    {
        Components.BuildContainer().Verify();
    }
}
