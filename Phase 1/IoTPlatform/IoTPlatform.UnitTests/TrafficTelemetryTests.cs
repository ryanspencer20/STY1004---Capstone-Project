using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class TrafficTelemetryTests
    {
        [TestMethod]
        public void Record_ValidInputs_StoresExpectedValues()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            DateTimeOffset before = DateTimeOffset.UtcNow;

            // Act
            TrafficTelemetry result = TrafficTelemetry.Record(
                device, Approach.NorthSouth, 5, 40m);

            DateTimeOffset after = DateTimeOffset.UtcNow;

            // Assert
            Assert.AreEqual(device, result.DeviceSerial);
            Assert.AreEqual(Approach.NorthSouth, result.Approach);
            Assert.AreEqual(5, result.VehicleCount);
            Assert.AreEqual(40m, result.SpeedKph);
            Assert.IsTrue(result.RecordedAt >= before);
            Assert.IsTrue(result.RecordedAt <= after);
        }

        [TestMethod]
        public void Record_ZeroVehicleCount_IsAccepted()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act
            TrafficTelemetry result = TrafficTelemetry.Record(
                device, Approach.EastWest, 0, 40m);

            // Assert
            Assert.AreEqual(0, result.VehicleCount);
            Assert.AreEqual(Approach.EastWest, result.Approach);
        }

        [TestMethod]
        public void Record_NegativeVehicleCount_ThrowsArgumentException()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                TrafficTelemetry.Record(device, Approach.NorthSouth, -1, 40m);
            });
        }

        [TestMethod]
        public void Record_ZeroSpeed_ThrowsArgumentException()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                TrafficTelemetry.Record(device, Approach.NorthSouth, 5, 0m);
            });
        }

        [TestMethod]
        public void Record_NegativeSpeed_ThrowsArgumentException()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            //Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                TrafficTelemetry.Record(device, Approach.NorthSouth, 5, -1m);
            });
        }

        [TestMethod]
        public void Record_MissingDevice_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                TrafficTelemetry.Record(null!, Approach.NorthSouth, 5, 40m);
            });
        }

        [TestMethod]
        public void Record_InvalidApproach_ThrowsArgumentException()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                TrafficTelemetry.Record(device, (Approach)99, 5, 40m);
            });
        }
    }
}
