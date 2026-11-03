using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class TrafficTelemetryTests
    {

        [TestMethod]
        [DataRow(Approach.NorthSouth, 5)]
        [DataRow(Approach.EastWest, 0)]
        public void Record_ValidInputs_StoresExpectedValues(Approach approach, int count)
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            DateTimeOffset before = DateTimeOffset.UtcNow;

            // Act
            TrafficTelemetry result = TrafficTelemetry.Record(
                device, approach, count, 40m);

            // Assert
            DateTimeOffset after = DateTimeOffset.UtcNow;
            Assert.AreEqual(device, result.DeviceSerial);
            Assert.AreEqual(approach, result.Approach);
            Assert.AreEqual(count, result.VehicleCount);
            Assert.AreEqual(40m, result.SpeedKph);
            Assert.IsTrue(result.RecordedAt >= before);
            Assert.IsTrue(result.RecordedAt <= after);
        }

        [TestMethod]
        [DataRow(-1, 40)]
        [DataRow(5, 0)]
        [DataRow(5, -1)]
        public void Record_InvalidCountOrSpeed_Throws(int count, int speed)
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            decimal speedKph = speed;

            // Act
            Action act = () => TrafficTelemetry.Record(
                device, Approach.NorthSouth, count, speedKph);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void Record_MissingDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act
            Action act = () => TrafficTelemetry.Record(
                device, Approach.NorthSouth, 5, 40m);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void Record_InvalidApproach_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act
            Action act = () => TrafficTelemetry.Record(
                device, (Approach)99, 5, 40m);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }
    }
}
