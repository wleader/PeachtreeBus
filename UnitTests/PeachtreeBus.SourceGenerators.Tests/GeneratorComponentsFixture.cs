using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PeachtreeBus.SourceGenerators.Tests;

[TestClass]
public class GeneratorComponentsFixture
{
    [TestMethod]
    public void When_VerifyContainer_Then_DoesNotThrow()
    {
        GeneratorComponents.BuildContainer().Verify();
    }
}
