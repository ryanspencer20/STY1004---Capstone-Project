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
            SerialNumber serial = SerialNumber.Parse("INT-001");

            // Act
            IntersectionDevice device = IntersectionDevice.Register(
                serial, "Paris and Cedar");

            // Assert
            Assert.AreEqual(serial, device.Serial);
            Assert.AreEqual("Paris and Cedar", device.DisplayName);
            Assert.AreEqual(DeviceLifecycle.Commissioned, device.Status);
            Assert.AreEqual(VehicleSignalState.Red, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, device.EastWestVehicle);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, device.EastWestPedestrian);
        }

        [TestMethod]
        public void Register_InvalidInputs_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                IntersectionDevice.Register(null!, "Paris and Cedar");
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                IntersectionDevice.Register(serial, "");
            });
        }
        [TestMethod]
        public void Lifecycle_ActivatesAndDecommissions()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(
                serial, "Test Intersection");

            // Act
            device.Activate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Active, device.Status);

            // Act
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);
            device.Deactivate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Decommissioned, device.Status);
            Assert.AreEqual(VehicleSignalState.Red, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, device.EastWestVehicle);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, device.EastWestPedestrian);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.Activate();
            });
        }

        [TestMethod]
        public void ChangePhase_ValidSequence_Works()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();

            // Act
            device.StartAdvancedGreen(Approach.NorthSouth);

            // Assert
            Assert.AreEqual(
                VehicleSignalState.AdvancedGreen, device.NorthSouthVehicle);

            // Act
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Yellow);
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);
            device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Assert
            Assert.AreEqual(VehicleSignalState.Red, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Green, device.EastWestVehicle);
        }

        [TestMethod]
        public void ChangePhase_UnsafeChanges_Throw()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);
            });

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);
            });

            // Assert
            Assert.AreEqual(VehicleSignalState.Green, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, device.EastWestVehicle);
        }

        [TestMethod]
        public void SetPedestrian_FollowsSequenceAndBlocksTraffic()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();

            // Act
            device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.Walk);

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.Walk, device.NorthSouthPedestrian);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);
            });

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.DontWalk);
            });

            // Act
            device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.FlashingDontWalk);

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.FlashingDontWalk,
                device.NorthSouthPedestrian);

            // Act
            device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.DontWalk);
            device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.Walk);
            });

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
        }

        [TestMethod]
        public void InactiveDevice_CannotChangeSignals()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);
            });

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.Walk);
            });
        }

        [TestMethod]
        public void AddRule_RejectsWrongDeviceAndDuplicateApproach()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(
                serial, "Test Intersection");

            SignalRule rule = SignalRule.Create(
                serial, Approach.NorthSouth, 10, 30, 3, 5, 8);
            SignalRule duplicate = SignalRule.Create(
                serial, Approach.NorthSouth, 15, 40, 3, 5, 10);
            SignalRule wrongDevice = SignalRule.Create(
                SerialNumber.Parse("INT-002"),
                Approach.EastWest, 10, 30, 3, 5, 8);

            // Act
            device.AddRule(rule);

            // Assert
            Assert.AreEqual(1, device.Rules.Count);
            Assert.AreEqual(rule, device.Rules[0]);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                device.AddRule(duplicate);
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                device.AddRule(wrongDevice);
            });

            // Assert
            Assert.AreEqual(1, device.Rules.Count);
        }
    }
}