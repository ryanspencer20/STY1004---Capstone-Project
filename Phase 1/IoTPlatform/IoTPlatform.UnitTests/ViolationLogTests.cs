using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class ViolationLogTests
    {
        [TestMethod]
        public void LogRedLight_ValidInput_CreatesLog()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act
            ViolationLog result = ViolationLog.LogRedLight(
                device, VehicleSignalState.Red);

            // Assert
            Assert.AreEqual(device, result.DeviceSerial);
            Assert.AreEqual(ViolationType.RedLight, result.Type);
            Assert.AreEqual(VehicleSignalState.Red, result.SignalAtTime);
        }

        [TestMethod]
        public void LogSpeeding_ValidInput_CreatesLog()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act
            ViolationLog result = ViolationLog.LogSpeeding(
                device, 70m, VehicleSignalState.Green);

            // Assert
            Assert.AreEqual(ViolationType.Speeding, result.Type);
            Assert.AreEqual((decimal?)70m, result.SpeedKph);
        }

        [TestMethod]
        public void LogRedLight_NotRed_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act
            Action act = () =>
            {
                ViolationLog.LogRedLight(device, VehicleSignalState.Green);
            };

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void LogRedLight_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act
            Action act = () =>
            {
                ViolationLog.LogRedLight(device, VehicleSignalState.Red);
            };

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void LogSpeeding_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act
            Action act = () =>
            {
                ViolationLog.LogSpeeding(device, 70m, VehicleSignalState.Green);
            };

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public void LogSpeeding_InvalidSpeed_Throws(int speed)
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");
            decimal speedKph = speed;

            // Act
            Action act = () => ViolationLog.LogSpeeding(
                device, speedKph, VehicleSignalState.Green);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void LogSpeeding_InvalidSignal_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act
            Action act = () =>
            {
                ViolationLog.LogSpeeding(device, 70m, (VehicleSignalState)99);
            };

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }
    }
}