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
                device, VehicleSignalState.Green, 70m);

            // Assert
            Assert.AreEqual(ViolationType.Speeding, result.Type);
            Assert.AreEqual((decimal?)70m, result.SpeedKph);
        }

        [TestMethod]
        public void LogRedLight_NotRed_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                ViolationLog.LogRedLight(device, VehicleSignalState.Green);
            });
        }

        [TestMethod]
        public void LogRedLight_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                ViolationLog.LogRedLight(device, VehicleSignalState.Red);
            });
        }

        [TestMethod]
        public void LogSpeeding_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                ViolationLog.LogSpeeding(device, VehicleSignalState.Green, 70m);
            });
        }

        [TestMethod]
        public void LogSpeeding_ZeroSpeed_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                ViolationLog.LogSpeeding(device, VehicleSignalState.Green, 0m);
            });
        }

        [TestMethod]
        public void LogSpeeding_NegativeSpeed_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                ViolationLog.LogSpeeding(device, VehicleSignalState.Green, -1m);
            });
        }

        [TestMethod]
        public void LogSpeeding_InvalidSignal_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("CAM-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                ViolationLog.LogSpeeding(device, (VehicleSignalState)99, 70m);
            });
        }
    }
}