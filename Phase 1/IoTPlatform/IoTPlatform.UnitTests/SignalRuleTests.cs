using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class SignalRuleTests
    {

        [TestMethod]
        public void Create_ValidInputs_StoresSettings()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Assert
            Assert.AreEqual(device, rule.LocationSerial);
            Assert.AreEqual(Approach.NorthSouth, rule.Approach);
            Assert.AreEqual(10, rule.MinGreenSeconds);
            Assert.AreEqual(30, rule.MaxGreenSeconds);
            Assert.AreEqual(3, rule.YellowSeconds);
            Assert.AreEqual(5, rule.AdvancedGreenSeconds);
            Assert.AreEqual(8, rule.CountOverrideThreshold);
            Assert.IsTrue(rule.IsEnabled);
        }

        [TestMethod]
        [DataRow(10, 30, 0, 5, 8)]
        [DataRow(40, 30, 3, 5, 8)]
        [DataRow(10, 30, 3, 5, 0)]
        public void Create_InvalidSettings_Throws(
            int minGreen, int maxGreen, int yellow, int advancedGreen, int threshold)
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act
            Action act = () => SignalRule.Create(
                device, Approach.NorthSouth, minGreen, maxGreen,
                yellow, advancedGreen, threshold);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void Create_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act
            Action act = () => SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void Create_InvalidApproach_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act
            Action act = () => SignalRule.Create(
                device, (Approach)99, 10, 30, 3, 5, 8);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        [DataRow("INT-001", Approach.NorthSouth, 8, true, true)]
        [DataRow("INT-001", Approach.NorthSouth, 7, true, false)]
        [DataRow("INT-001", Approach.NorthSouth, 8, false, false)]
        [DataRow("INT-002", Approach.NorthSouth, 8, true, false)]
        [DataRow("INT-001", Approach.EastWest, 8, true, false)]
        public void ShouldOverride_ReturnsExpectedResult(
            string readingSerial, Approach approach, int count,
            bool enabled, bool expected)
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8, enabled);
            TrafficTelemetry reading = TrafficTelemetry.Record(
                SerialNumber.Parse(readingSerial), approach, count, 40m);

            // Act
            bool result = rule.ShouldOverride(reading);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShouldOverride_NullReading_Throws()
        {
            // Arrange
            SignalRule rule = SignalRule.Create(
                SerialNumber.Parse("INT-001"), Approach.NorthSouth,
                10, 30, 3, 5, 8);

            // Act
            Action act = () => rule.ShouldOverride(null!);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        [DataRow(VehicleSignalState.AdvancedGreen, 4, true, VehicleSignalState.AdvancedGreen)]
        [DataRow(VehicleSignalState.AdvancedGreen, 5, true, VehicleSignalState.Green)]
        [DataRow(VehicleSignalState.Green, 29, true, VehicleSignalState.Green)]
        [DataRow(VehicleSignalState.Green, 30, true, VehicleSignalState.Yellow)]
        [DataRow(VehicleSignalState.Yellow, 2, true, VehicleSignalState.Yellow)]
        [DataRow(VehicleSignalState.Yellow, 3, true, VehicleSignalState.Red)]
        [DataRow(VehicleSignalState.Green, 30, false, VehicleSignalState.Green)]
        [DataRow(VehicleSignalState.Red, 30, true, VehicleSignalState.Red)]
        public void NextState_ReturnsExpectedState(
            VehicleSignalState current, int elapsed, bool enabled,
            VehicleSignalState expected)
        {
            // Arrange
            SignalRule rule = SignalRule.Create(
                SerialNumber.Parse("INT-001"), Approach.NorthSouth,
                10, 30, 3, 5, 8, enabled);

            // Act
            VehicleSignalState result = rule.NextState(current, elapsed);

            // Assert
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow((VehicleSignalState)99, 0)]
        [DataRow(VehicleSignalState.Green, -1)]
        public void NextState_InvalidInput_Throws(
            VehicleSignalState current, int elapsed)
        {
            // Arrange
            SignalRule rule = SignalRule.Create(
                SerialNumber.Parse("INT-001"), Approach.NorthSouth,
                10, 30, 3, 5, 8);

            // Act
            Action act = () => rule.NextState(current, elapsed);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }
    }
}
