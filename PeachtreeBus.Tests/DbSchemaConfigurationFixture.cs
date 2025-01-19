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
            var expected = new SchemaName("TestSchema");
            IDbSchemaConfiguration schema = new DbSchemaConfiguration(expected);
            Assert.AreEqual(expected, schema.Schema);
        }

        [TestMethod]
        public void DbSchemaConfiguration_UsesCorrectDefault()
        {
            IDbSchemaConfiguration schema = new DbSchemaConfiguration(null);
            Assert.AreEqual("PeachtreeBus", schema.Schema.Value);
        }
    }
}
