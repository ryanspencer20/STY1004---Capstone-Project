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
            Assert.AreEqual(device, rule.DeviceSerial);
            Assert.AreEqual(Approach.NorthSouth, rule.Approach);
            Assert.AreEqual(10, rule.MinGreenSeconds);
            Assert.AreEqual(30, rule.MaxGreenSeconds);
            Assert.AreEqual(3, rule.YellowSeconds);
            Assert.AreEqual(5, rule.AdvancedGreenSeconds);
            Assert.AreEqual(8, rule.CountOverrideThreshold);
            Assert.IsTrue(rule.IsEnabled);
        }

        [TestMethod]
        public void ShouldOverride_CountAtThreshold_ReturnsTrue()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);
            TrafficTelemetry reading = TrafficTelemetry.Record(
                device, Approach.NorthSouth, 8, 40m);

            // Act
            bool result = rule.ShouldOverride(reading);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void NextState_GreenAtMaximum_ReturnsYellow()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Act
            VehicleSignalState result = rule.NextState(
                VehicleSignalState.Green, 30);

            // Assert
            Assert.AreEqual(VehicleSignalState.Yellow, result);
        }

        [TestMethod]
        public void Create_ZeroDuration_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SignalRule.Create(
                    device, Approach.NorthSouth, 10, 30, 0, 5, 8);
            });
        }

        [TestMethod]
        public void Create_MinimumAboveMaximum_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SignalRule.Create(
                    device, Approach.NorthSouth, 40, 30, 3, 5, 8);
            });
        }

        [TestMethod]
        public void Create_NullDevice_Throws()
        {
            // Arrange
            SerialNumber device = null!;

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                SignalRule.Create(
                    device, Approach.NorthSouth, 10, 30, 3, 5, 8);
            });
        }

        [TestMethod]
        public void Create_InvalidApproach_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SignalRule.Create(
                    device, (Approach)99, 10, 30, 3, 5, 8);
            });
        }

        [TestMethod]
        public void Create_ZeroThreshold_Throws()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                SignalRule.Create(
                    device, Approach.NorthSouth, 10, 30, 3, 5, 0);
            });
        }

        [TestMethod]
        public void ShouldOverride_CountBelowThreshold_ReturnsFalse()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);
            TrafficTelemetry reading = TrafficTelemetry.Record(
                device, Approach.NorthSouth, 7, 40m);

            // Act
            bool result = rule.ShouldOverride(reading);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldOverride_DisabledRule_ReturnsFalse()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8, false);
            TrafficTelemetry reading = TrafficTelemetry.Record(
                device, Approach.NorthSouth, 8, 40m);

            // Act
            bool result = rule.ShouldOverride(reading);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NextState_FollowsConfiguredTimes()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Act
            VehicleSignalState earlyAdvanced = rule.NextState(
                VehicleSignalState.AdvancedGreen, 4);
            VehicleSignalState finishedAdvanced = rule.NextState(
                VehicleSignalState.AdvancedGreen, 5);
            VehicleSignalState earlyGreen = rule.NextState(
                VehicleSignalState.Green, 29);
            VehicleSignalState earlyYellow = rule.NextState(
                VehicleSignalState.Yellow, 2);
            VehicleSignalState finishedYellow = rule.NextState(
                VehicleSignalState.Yellow, 3);

            // Assert
            Assert.AreEqual(VehicleSignalState.AdvancedGreen, earlyAdvanced);
            Assert.AreEqual(VehicleSignalState.Green, finishedAdvanced);
            Assert.AreEqual(VehicleSignalState.Green, earlyGreen);
            Assert.AreEqual(VehicleSignalState.Yellow, earlyYellow);
            Assert.AreEqual(VehicleSignalState.Red, finishedYellow);
        }

        [TestMethod]
        public void ShouldOverride_UnrelatedReading_ReturnsFalse()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            TrafficTelemetry otherDevice = TrafficTelemetry.Record(
                SerialNumber.Parse("INT-002"), Approach.NorthSouth, 8, 40m);

            TrafficTelemetry otherApproach = TrafficTelemetry.Record(
                device, Approach.EastWest, 8, 40m);

            // Act
            bool wrongDevice = rule.ShouldOverride(otherDevice);
            bool wrongApproach = rule.ShouldOverride(otherApproach);

            // Assert
            Assert.IsFalse(wrongDevice);
            Assert.IsFalse(wrongApproach);
        }

        [TestMethod]
        public void NextState_DisabledOrRed_DoesNotChange()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule disabled = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8, false);
            SignalRule enabled = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Act
            VehicleSignalState disabledResult = disabled.NextState(
                VehicleSignalState.Green, 30);
            VehicleSignalState redResult = enabled.NextState(
                VehicleSignalState.Red, 30);

            // Assert
            Assert.AreEqual(VehicleSignalState.Green, disabledResult);
            Assert.AreEqual(VehicleSignalState.Red, redResult);
        }

        [TestMethod]
        public void RuleMethods_InvalidInputs_Throw()
        {
            // Arrange
            SerialNumber device = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                device, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                rule.ShouldOverride(null!);
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                rule.NextState((VehicleSignalState)99, 0);
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                rule.NextState(VehicleSignalState.Green, -1);
            });
        }
    }
}