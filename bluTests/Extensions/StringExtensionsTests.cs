using blu.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace bluTests.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void UnCamelCaseTest_TestForNull()
        {
            var result = ((string) null).UnCamelCase();

            Assert.IsNull(result);
        }


        [TestMethod]
        public void UnCamelCaseTest_TestForEmpty()
        {
            var str = string.Empty;

            var result = str.UnCamelCase();

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void UnCamelCaseTest_Simple()
        {
            const string str = "Simple";

            var result = str.UnCamelCase();

            Assert.AreEqual("Simple", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_simple()
        {
            const string str = "simple";

            var result = str.UnCamelCase();

            Assert.AreEqual("simple", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_TwoWords()
        {
            const string str = "TwoWords";

            var result = str.UnCamelCase();

            Assert.AreEqual("Two Words", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_ThreeWords()
        {
            const string str = "ThreeMoreWords";

            var result = str.UnCamelCase();

            Assert.AreEqual("Three More Words", result);
        }
    }
}