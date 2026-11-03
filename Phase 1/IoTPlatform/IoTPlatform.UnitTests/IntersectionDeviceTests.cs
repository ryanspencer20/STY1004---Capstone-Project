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
            Assert.AreEqual(PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
            Assert.AreEqual(PedestrianSignalState.DontWalk, device.EastWestPedestrian);
        }

        [TestMethod]
        public void Register_NullSerial_Throws()
        {
            // Arrange
            SerialNumber serial = null!;

            // Act
            Action act = () => IntersectionDevice.Register(serial, "Paris and Cedar");

            // Assert
            Assert.ThrowsExactly<ArgumentNullException>(act);
        }

        [TestMethod]
        public void Register_EmptyName_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");

            // Act
            Action act = () => IntersectionDevice.Register(serial, "");

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
        }

        [TestMethod]
        public void Activate_CommissionedDevice_BecomesActive()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");

            // Act
            device.Activate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Active, device.Status);
        }

        [TestMethod]
        public void Deactivate_ActiveDevice_SetsSafeStates()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);

            // Act
            device.Deactivate();

            // Assert
            Assert.AreEqual(DeviceLifecycle.Decommissioned, device.Status);
            Assert.AreEqual(VehicleSignalState.Red, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, device.EastWestVehicle);
            Assert.AreEqual(PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
            Assert.AreEqual(PedestrianSignalState.DontWalk, device.EastWestPedestrian);
        }

        [TestMethod]
        public void Activate_DecommissionedDevice_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.Deactivate();

            // Act
            Action act = () => device.Activate();

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void StartAdvancedGreen_SafeDevice_SetsAdvancedGreen()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();

            // Act
            device.StartAdvancedGreen(Approach.NorthSouth);

            // Assert
            Assert.AreEqual(VehicleSignalState.AdvancedGreen, device.NorthSouthVehicle);
        }

        [TestMethod]
        public void ChangePhase_NorthSouthStopped_AllowsEastWestGreen()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.StartAdvancedGreen(Approach.NorthSouth);
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Yellow);
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);

            // Act
            device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Assert
            Assert.AreEqual(VehicleSignalState.Red, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Green, device.EastWestVehicle);
        }

        [TestMethod]
        [DataRow(Approach.EastWest, VehicleSignalState.Green)]
        [DataRow(Approach.NorthSouth, VehicleSignalState.Red)]
        public void ChangePhase_UnsafeChange_Throws(
            Approach approach, VehicleSignalState nextState)
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);

            // Act
            Action act = () => device.ChangePhase(approach, nextState);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
            Assert.AreEqual(VehicleSignalState.Green, device.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, device.EastWestVehicle);
        }

        [TestMethod]
        [DataRow(PedestrianSignalState.DontWalk, PedestrianSignalState.Walk)]
        [DataRow(PedestrianSignalState.Walk, PedestrianSignalState.FlashingDontWalk)]
        [DataRow(PedestrianSignalState.FlashingDontWalk, PedestrianSignalState.DontWalk)]
        public void SetPedestrian_ValidTransition_ChangesState(
            PedestrianSignalState current, PedestrianSignalState nextState)
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();

            if (current != PedestrianSignalState.DontWalk)
            {
                device.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.Walk);
            }

            if (current == PedestrianSignalState.FlashingDontWalk)
            {
                device.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.FlashingDontWalk);
            }

            // Act
            device.SetPedestrian(Approach.NorthSouth, nextState);

            // Assert
            Assert.AreEqual(nextState, device.NorthSouthPedestrian);
        }

        [TestMethod]
        public void ChangePhase_PedestriansWalking_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.Walk);

            // Act
            Action act = () => device.ChangePhase(
                Approach.EastWest, VehicleSignalState.Green);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void SetPedestrian_SkipsFlashingState_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.Walk);

            // Act
            Action act = () => device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.DontWalk);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void SetPedestrian_TrafficMoving_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");
            device.Activate();
            device.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.Walk);
            device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.FlashingDontWalk);
            device.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.DontWalk);
            device.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Act
            Action act = () => device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.Walk);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
            Assert.AreEqual(PedestrianSignalState.DontWalk, device.NorthSouthPedestrian);
        }

        [TestMethod]
        public void ChangePhase_InactiveDevice_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");

            // Act
            Action act = () => device.ChangePhase(
                Approach.NorthSouth, VehicleSignalState.Green);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void SetPedestrian_InactiveDevice_Throws()
        {
            // Arrange
            IntersectionDevice device = IntersectionDevice.Register(
                SerialNumber.Parse("INT-001"), "Test Intersection");

            // Act
            Action act = () => device.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.Walk);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void AddRule_MatchingDevice_AddsRule()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(serial, "Test Intersection");
            SignalRule rule = SignalRule.Create(
                serial, Approach.NorthSouth, 10, 30, 3, 5, 8);

            // Act
            device.AddRule(rule);

            // Assert
            Assert.AreEqual(1, device.Rules.Count);
            Assert.AreEqual(rule, device.Rules[0]);
        }

        [TestMethod]
        public void AddRule_DuplicateApproach_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(serial, "Test Intersection");
            device.AddRule(SignalRule.Create(
                serial, Approach.NorthSouth, 10, 30, 3, 5, 8));
            SignalRule duplicate = SignalRule.Create(
                serial, Approach.NorthSouth, 15, 40, 3, 5, 10);

            // Act
            Action act = () => device.AddRule(duplicate);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
            Assert.AreEqual(1, device.Rules.Count);
        }

        [TestMethod]
        public void AddRule_WrongDevice_Throws()
        {
            // Arrange
            SerialNumber serial = SerialNumber.Parse("INT-001");
            IntersectionDevice device = IntersectionDevice.Register(serial, "Test Intersection");
            device.AddRule(SignalRule.Create(
                serial, Approach.NorthSouth, 10, 30, 3, 5, 8));
            SignalRule wrongDevice = SignalRule.Create(
                SerialNumber.Parse("INT-002"), Approach.EastWest, 10, 30, 3, 5, 8);

            // Act
            Action act = () => device.AddRule(wrongDevice);

            // Assert
            Assert.ThrowsExactly<ArgumentException>(act);
            Assert.AreEqual(1, device.Rules.Count);
        }
    }
}
