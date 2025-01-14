using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeachtreeBus.Data;

namespace PeachtreeBus.Tests
{
    [TestClass]
    public class DbSchemaConfigurationFixture
    {
        [TestMethod]
        public void DbSchemaConfiguration_ReturnsSchemaNameProvidedToConstructor()
        {
            IDbSchemaConfiguration schema = new DbSchemaConfiguration("TestSchema");
            Assert.AreEqual("TestSchema", schema.Schema);
        }

        [TestMethod]
        public void DbSchemaConfiguration_UsesCorrectDefault()
        {
            IDbSchemaConfiguration schema = new DbSchemaConfiguration();
            Assert.AreEqual("PeachtreeBus", schema.Schema);
        }
    }
}
