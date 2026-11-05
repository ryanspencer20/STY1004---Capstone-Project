using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class IntersectionDeviceTests
    {
        [TestMethod]
        public void Register_SetsDetailsAndSafeStates()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("CAM-001");
            SerialNumber location = SerialNumber.Parse("INT-001");
            // Act
            IntersectionDevice device = IntersectionDevice.Register(
                serial, location, "Paris and Cedar", FacingDirection.North, "1.0.0");

            // Assert
            Assert.AreEqual(serial, device.Serial);
            Assert.AreEqual("Paris and Cedar", device.DisplayName);
            Assert.AreEqual(FacingDirection.North, device.FacingDirection);
            Assert.AreEqual("1.0.0", device.FirmwareVersion);
            Assert.AreEqual(DeviceLifecycle.Commissioned, device.Status);
        }

        [TestMethod]
        public void Register_InvalidInputs_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("CAM-001");
            SerialNumber location = SerialNumber.Parse("INT-001");
            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                IntersectionDevice.Register(null!, location, "Paris and Cedar", FacingDirection.North, "1.0.0");
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                IntersectionDevice.Register(serial, location, "", FacingDirection.North, "1.0.0");
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                IntersectionDevice.Register(serial, location, "Paris and Cedar", FacingDirection.North, "   ");
            });
        }

        [TestMethod]
        public void Lifecycle_ActivatesAndDecommissions()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("CAM-001");
            SerialNumber location = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(
                serial, location, "Test Intersection", FacingDirection.North, "1.0.0");

            // Act
            device.Activate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Active, device.Status);

            // Act
            device.Deactivate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Decommissioned, device.Status);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.Activate();
            });
        }

        [TestMethod]
        public void Register_NullLocation_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("CAM-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                IntersectionDevice.Register(serial, null!, "Paris and Cedar", FacingDirection.North, "1.0.0");
            });
        }

        [TestMethod]
        public void Deactivate_FromCommissioned_Throws()
        {
            // Arrange — a freshly-registered device is Commissioned, not
            // Active/Degraded, so it cannot be deactivated directly.
            SerialNumber serial = SerialNumber.Parse("CAM-001");
            SerialNumber location = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(
                serial, location, "Test Intersection", FacingDirection.North, "1.0.0");

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.Deactivate();
            });
        }

        [TestMethod]
        public void Restore_SetsProvidedStatusAndCommissionedDate()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("CAM-001");
            SerialNumber location = SerialNumber.Parse("INT-001");
            DateTime commissionedDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            // Act — unlike Register (always Commissioned/now), Restore must
            // rehydrate whatever status and date actually came from storage.
            IntersectionDevice device = IntersectionDevice.Restore(
                serial, location, "Paris and Cedar", FacingDirection.North, "1.0.0",
                DeviceLifecycle.Active, commissionedDate);

            // Assert
            Assert.AreEqual(serial, device.Serial);
            Assert.AreEqual(location, device.LocationSerial);
            Assert.AreEqual(DeviceLifecycle.Active, device.Status);
            Assert.AreEqual(commissionedDate, device.CommissionedDate);
        }
    }
}