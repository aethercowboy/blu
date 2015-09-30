using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blu.Extensions.Tests
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void UnCamelCaseTest_TestForNull()
        {
            String str = null;

            var result = str.UnCamelCase();

            Assert.IsNull(result);
        }


        [TestMethod]
        public void UnCamelCaseTest_TestForEmpty()
        {
            String str = String.Empty;

            var result = str.UnCamelCase();

            Assert.AreEqual(String.Empty, result);
        }

        [TestMethod]
        public void UnCamelCaseTest_Simple()
        {
            String str = "Simple";

            var result = str.UnCamelCase();

            Assert.AreEqual("Simple", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_simple()
        {
            String str = "simple";

            var result = str.UnCamelCase();

            Assert.AreEqual("simple", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_TwoWords()
        {
            String str = "TwoWords";

            var result = str.UnCamelCase();

            Assert.AreEqual("Two Words", result);
        }

        [TestMethod]
        public void UnCamelCaseTest_ThreeWords()
        {
            String str = "ThreeMoreWords";

            var result = str.UnCamelCase();

            Assert.AreEqual("Three More Words", result);
        }
    }
}