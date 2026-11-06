using System;
using IoTPlatform.Application;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class ViolationAlertsTests
    {
        [TestMethod]
        public void EvaluateIsRedLightViolation_NullDevice_Throws()
        {
            // Arrange
            var alerts = new ViolationAlerts(new SpyViolationLogRepository());

            // Act
            Action act = () => alerts.EvaluateIsRedLightViolation(null!, VehicleSignalState.Red);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void EvaluateIsRedLightViolation_SignalIsRed_LogsAndReturnsViolation()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsRedLightViolation(device, VehicleSignalState.Red, 55m, "ABC123");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViolationType.RedLight, result!.Type);
            Assert.AreEqual(device, result.DeviceSerial);
            Assert.AreEqual(VehicleSignalState.Red, result.SignalAtTime);
            Assert.AreEqual("ABC123", result.Plate);
            Assert.AreSame(result, repository.SavedViolation);
            Assert.AreEqual(1, repository.SaveCount);
        }

        [TestMethod]
        public void EvaluateIsRedLightViolation_SignalNotRed_ReturnsNullAndDoesNotSave()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsRedLightViolation(device, VehicleSignalState.Green);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(0, repository.SaveCount);
        }

        [TestMethod]
        public void EvaluateIsSpeedingViolation_NullDevice_Throws()
        {
            // Arrange
            var alerts = new ViolationAlerts(new SpyViolationLogRepository());

            // Act
            Action act = () => alerts.EvaluateIsSpeedingViolation(null!, VehicleSignalState.Green, 85m);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void EvaluateIsSpeedingViolation_SpeedIsNull_ReturnsNullAndDoesNotSave()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsSpeedingViolation(device, VehicleSignalState.Green, null);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(0, repository.SaveCount);
        }

        [TestMethod]
        public void EvaluateIsSpeedingViolation_SpeedZeroOrNegative_ReturnsNullAndDoesNotSave()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsSpeedingViolation(device, VehicleSignalState.Green, -5m);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(0, repository.SaveCount);
        }

        [TestMethod]
        public void EvaluateIsSpeedingViolation_SpeedAtThreshold_ReturnsNullAndDoesNotSave()
        {
            // 70 km/h is the threshold itself (speed > 70 is required), not a violation yet.
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsSpeedingViolation(device, VehicleSignalState.Green, 70m);

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(0, repository.SaveCount);
        }

        [TestMethod]
        public void EvaluateIsSpeedingViolation_SpeedOverThreshold_LogsAndReturnsViolation()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repository = new SpyViolationLogRepository();
            var alerts = new ViolationAlerts(repository);

            // Act
            var result = alerts.EvaluateIsSpeedingViolation(device, VehicleSignalState.Green, 85.5m, "XYZ999");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViolationType.Speeding, result!.Type);
            Assert.AreEqual(device, result.DeviceSerial);
            Assert.AreEqual(85.5m, result.SpeedKph);
            Assert.AreEqual("XYZ999", result.Plate);
            Assert.AreSame(result, repository.SavedViolation);
            Assert.AreEqual(1, repository.SaveCount);
        }

        /// <summary>
        /// Records whatever violation gets saved, so tests can assert on it
        /// and confirm when SaveViolation was (or wasn't) called.
        /// </summary>
        private sealed class SpyViolationLogRepository : IViolationLogRepository
        {
            public ViolationLog? SavedViolation { get; private set; }
            public int SaveCount { get; private set; }

            public void SaveViolation(ViolationLog violation)
            {
                SavedViolation = violation;
                SaveCount++;
            }
        }
    }
}
