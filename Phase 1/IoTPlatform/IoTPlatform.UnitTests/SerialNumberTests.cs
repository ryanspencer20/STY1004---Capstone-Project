using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class SerialNumberTests
    {
        [TestMethod]
        public void Parse_ValidIntersectionSerial_ReturnsExpectedValue()
        {
            // Arrange
            string input = "INT-001";

            // Act
            SerialNumber result = SerialNumber.Parse(input);

            // Assert
            Assert.AreEqual(input, result.Value);
        }

        [TestMethod]
        public void Parse_InvalidPrefix_ThrowsArgumentException()
        {
            // Arrange
            string input = "ABC-001";

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {  
                SerialNumber.Parse(input);
            });
        }

        [TestMethod]
        public void Parse_ValidCameraSerial_ReturnsExpectedValue()
        {
            // Arrange
            string input = "CAM-123";

            // Act
            SerialNumber result = SerialNumber.Parse(input);

            // Assert
            Assert.AreEqual(input, result.Value);
        }

        [TestMethod]
        public void Parse_EmptyInput_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SerialNumber.Parse("");
            });
        }

        [TestMethod]
        public void Parse_WrongLength_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SerialNumber.Parse("INT-01");
            });
        }

        [TestMethod]
        public void Parse_NonDigitEnding_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SerialNumber.Parse("INT-0A1");
            });
        }

        [TestMethod]
        public void SerialNumbers_WithSameValue_AreEqual()
        {
            // Arrange
            SerialNumber first = SerialNumber.Parse("INT-001");
            SerialNumber second = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.AreEqual(first, second);
        }
    }
}
