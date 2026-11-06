using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class SerialNumberTests
    {

        [TestMethod]
        [DataRow("INT-001")]
        [DataRow("CAM-123")]
        public void Parse_ValidSerial_ReturnsExpectedValue(string input)
        {
            // Arrange
            string expected = input;

            // Act
            SerialNumber result = SerialNumber.Parse(input);

            // Assert
            Assert.AreEqual(expected, result.Value);
        }

        [TestMethod]
        [DataRow("ABC-001")]
        [DataRow("")]
        [DataRow("INT-01")]
        [DataRow("INT-0A1")]
        public void Parse_InvalidSerial_Throws(string input)
        {
            // Arrange
            string invalidSerial = input;

            // Act
            Action act = () => SerialNumber.Parse(invalidSerial);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void SerialNumbers_WithSameValue_AreEqual()
        {
            // Arrange
            SerialNumber first = SerialNumber.Parse("INT-001");
            SerialNumber second = SerialNumber.Parse("INT-001");

            // Act
            bool result = first.Equals(second);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
