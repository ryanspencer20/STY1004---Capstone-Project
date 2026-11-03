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
            var name = "Main St & 1st Ave";

            // Act
            actions.Execute(serial, name);

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

            // Act
            Action act = () => actions.Execute(serial, "   ");

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void Execute_ShouldThrow_WhenSerialIsBlank()
        {
            // Arrange
            var repository = new SpyIntersectionDeviceRepository();
            var actions = new DeviceActions(repository);

            // Act
            Action act = () => actions.Execute(SerialNumber.Parse(""), "Main St & 1st Ave");

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        /// <summary>
        /// Minimal in-memory test double that records whatever device
        /// is passed to SaveDevice, so tests can assert on it afterward
        /// without touching a real database.
        /// </summary>
        private sealed class SpyIntersectionDeviceRepository : IIntersectionDeviceRepository
        {
            public IntersectionDevice? SavedDevice { get; private set; }

            public IntersectionDevice? GetDeviceBySerial(SerialNumber serial) => null;

            public void SaveDevice(IntersectionDevice device)
            {
                SavedDevice = device;
            }
        }
    }
}