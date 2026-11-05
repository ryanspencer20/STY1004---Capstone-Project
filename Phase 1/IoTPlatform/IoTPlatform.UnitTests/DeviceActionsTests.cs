using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IoTPlatform.Application;
using IoTPlatform.Domain;
namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class DeviceActionsTests
    {
        [TestMethod]
        public void Execute_ShouldThrow_WhenRepositoryIsNull()
        {
            // Arrange
            IIntersectionDeviceRepository? repository = null;

            // Act
            Action act = () => new DeviceActions(repository!);

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void Execute_CallsRepositorySaveDevice_WithMatchingSerialAndName()
        {
            // Arrange
            var repository = new SpyIntersectionDeviceRepository();
            var actions = new DeviceActions(repository);
            var serial = SerialNumber.Parse("INT-001");

            var location = SerialNumber.Parse("INT-002");
            var name = "Main St & 1st Ave";

            // Act
            actions.ExecuteRegistration(serial, location, name, FacingDirection.North, "1.0.0");

            // Assert
            Assert.IsNotNull(repository.SavedDevice);
            Assert.AreEqual(serial, repository.SavedDevice!.Serial);
            Assert.AreEqual(name, repository.SavedDevice!.DisplayName);
        }

        [TestMethod]
        public void Execute_ShouldThrow_WhenNameIsBlank()
        {
            // Arrange
            var repository = new SpyIntersectionDeviceRepository();
            var actions = new DeviceActions(repository);
            var serial = SerialNumber.Parse("INT-001");
            var location = SerialNumber.Parse("INT-002");

            // Act
            Action act = () => actions.ExecuteRegistration(serial, location, "   ", FacingDirection.North, "1.0.0");

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void Execute_ShouldThrow_WhenSerialIsBlank()
        {
            // Arrange
            var repository = new SpyIntersectionDeviceRepository();
            var actions = new DeviceActions(repository);
            var location = SerialNumber.Parse("INT-002");

            // Act
            Action act = () => actions.ExecuteRegistration(SerialNumber.Parse(""), location, "Main St & 1st Ave", FacingDirection.North, "1.0.0");

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void GetDeviceBySerialNumber_Found_ReturnsDevice()
        {
            // Arrange
            var serial = SerialNumber.Parse("INT-001");
            var location = SerialNumber.Parse("INT-002");
            var device = IntersectionDevice.Register(serial, location, "Main St & 1st Ave", FacingDirection.North, "1.0.0");
            var repository = new SpyIntersectionDeviceRepository { DeviceToReturn = device };
            var actions = new DeviceActions(repository);

            // Act
            var result = actions.GetDeviceBySerialNumber(serial);

            // Assert
            Assert.AreSame(device, result);
        }

        [TestMethod]
        public void GetDeviceBySerialNumber_NotFound_Throws()
        {
            // Arrange
            var repository = new SpyIntersectionDeviceRepository();
            var actions = new DeviceActions(repository);
            var serial = SerialNumber.Parse("INT-001");

            // Act
            Action act = () => actions.GetDeviceBySerialNumber(serial);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void ExecuteUpdateDevice_Found_PersistsIncomingDeviceNotStaleExisting()
        {
            // Arrange — two distinct instances: one representing what's
            // currently stored, one representing the caller's new values.
            var serial = SerialNumber.Parse("INT-001");
            var location = SerialNumber.Parse("INT-002");
            var storedDevice = IntersectionDevice.Register(serial, location, "Main St & 1st Ave", FacingDirection.North, "1.0.0");
            var updatedDevice = IntersectionDevice.Register(serial, location, "Main St & 1st Ave", FacingDirection.South, "1.1.0");
            var repository = new SpyIntersectionDeviceRepository { DeviceToReturn = storedDevice };
            var actions = new DeviceActions(repository);

            // Act
            actions.ExecuteUpdateDevice(updatedDevice);

            // Assert — the new values must be what gets persisted, not the
            // stale record that was only fetched to confirm existence.
            Assert.AreSame(updatedDevice, repository.UpdatedDevice);
            Assert.AreNotSame(storedDevice, repository.UpdatedDevice);
        }

        [TestMethod]
        public void ExecuteUpdateDevice_NotFound_Throws()
        {
            // Arrange
            var serial = SerialNumber.Parse("INT-001");
            var location = SerialNumber.Parse("INT-002");
            var device = IntersectionDevice.Register(serial, location, "Main St & 1st Ave", FacingDirection.North, "1.0.0");
            var repository = new SpyIntersectionDeviceRepository(); // DeviceToReturn left null -> "not found"
            var actions = new DeviceActions(repository);

            // Act
            Action act = () => actions.ExecuteUpdateDevice(device);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        /// <summary>
        /// Minimal in-memory test double that records whatever device
        /// is passed to SaveDevice, so tests can assert on it afterward
        /// without touching a real database.
        /// </summary>
        private sealed class SpyIntersectionDeviceRepository : IIntersectionDeviceRepository
        {
            public IntersectionDevice? DeviceToReturn { get; set; }
            public IntersectionDevice? SavedDevice { get; private set; }
            public IntersectionDevice? UpdatedDevice { get; private set; }

            public IntersectionDevice? GetDeviceBySerial(SerialNumber serial) => DeviceToReturn;

            public void SaveDevice(IntersectionDevice device)
            {
                SavedDevice = device;
            }

            public void UpdateDevice(IntersectionDevice device)
            {
                UpdatedDevice = device;
            }
        }
    }
}