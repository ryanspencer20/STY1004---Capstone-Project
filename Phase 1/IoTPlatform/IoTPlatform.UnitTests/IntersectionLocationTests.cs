using System;
using IoTPlatform.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IoTPlatform.UnitTests
{
    [TestClass]
    public class IntersectionLocationTests
    {
        [TestMethod]
        public void Register_SetsDetailsAndSafeStates()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");

            // Act
            IntersectionLocation location = IntersectionLocation.Register(
                locationSerial, "Paris and Cedar", 46.49, -80.99);

            // Assert
            Assert.AreEqual(locationSerial, location.LocationSerial);
            Assert.AreEqual("Paris and Cedar", location.LocationName);
            Assert.AreEqual(46.49, location.Latitude);
            Assert.AreEqual(-80.99, location.Longitude);
            Assert.AreEqual(VehicleSignalState.Red, location.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, location.EastWestVehicle);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, location.NorthSouthPedestrian);
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, location.EastWestPedestrian);
        }

        [TestMethod]
        public void Register_NullSerial_Throws()
        {
            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                IntersectionLocation.Register(null!);
            });
        }

        [TestMethod]
        public void ChangePhase_ValidSequence_Works()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act
            location.StartAdvancedGreen(Approach.NorthSouth);

            // Assert
            Assert.AreEqual(
                VehicleSignalState.AdvancedGreen, location.NorthSouthVehicle);

            // Act
            location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);
            location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Yellow);
            location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);
            location.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Assert
            Assert.AreEqual(VehicleSignalState.Red, location.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Green, location.EastWestVehicle);
        }

        [TestMethod]
        public void ChangePhase_UnsafeChanges_Throw()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);
            location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Green);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.ChangePhase(Approach.EastWest, VehicleSignalState.Green);
            });

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);
            });

            // Assert
            Assert.AreEqual(VehicleSignalState.Green, location.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, location.EastWestVehicle);
        }

        [TestMethod]
        public void SetPedestrian_FollowsSequenceAndBlocksTraffic()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act
            location.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.Walk);

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.Walk, location.NorthSouthPedestrian);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.ChangePhase(Approach.EastWest, VehicleSignalState.Green);
            });

            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.DontWalk);
            });

            // Act
            location.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.FlashingDontWalk);

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.FlashingDontWalk,
                location.NorthSouthPedestrian);

            // Act
            location.SetPedestrian(
                Approach.NorthSouth, PedestrianSignalState.DontWalk);
            location.ChangePhase(Approach.EastWest, VehicleSignalState.Green);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.SetPedestrian(
                    Approach.NorthSouth, PedestrianSignalState.Walk);
            });

            // Assert
            Assert.AreEqual(
                PedestrianSignalState.DontWalk, location.NorthSouthPedestrian);
        }

        [TestMethod]
        public void AddRule_RejectsWrongLocationAndDuplicateApproach()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            SerialNumber otherLocationSerial = SerialNumber.Parse("INT-002");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            SignalRule rule = SignalRule.Create(
                locationSerial, Approach.NorthSouth, 10, 30, 3, 5, 8);
            SignalRule duplicate = SignalRule.Create(
                locationSerial, Approach.NorthSouth, 15, 40, 3, 5, 10);
            SignalRule wrongLocation = SignalRule.Create(
                otherLocationSerial,
                Approach.EastWest, 10, 30, 3, 5, 8);

            // Act
            location.AddRule(rule);

            // Assert
            Assert.AreEqual(1, location.Rules.Count);
            Assert.AreEqual(rule, location.Rules[0]);

            // Act and Assert
            Assert.ThrowsExactly<InvalidOperationException>(() =>
            {
                location.AddRule(duplicate);
            });

            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                location.AddRule(wrongLocation);
            });

            // Assert
            Assert.AreEqual(1, location.Rules.Count);
        }

        [TestMethod]
        public void Restore_SetsProvidedStateAndRules()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            SignalRule rule = SignalRule.Create(
                locationSerial, Approach.NorthSouth, 10, 30, 3, 5, 8);
            DateTime timestamp = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);

            // Act — unlike Register (always safe/default state), Restore must
            // rehydrate whatever signal state and rules actually came from storage.
            IntersectionLocation location = IntersectionLocation.Restore(
                locationSerial,
                "Paris and Cedar",
                46.49,
                -80.99,
                VehicleSignalState.Green,
                VehicleSignalState.Red,
                PedestrianSignalState.DontWalk,
                PedestrianSignalState.DontWalk,
                new[] { rule },
                timestamp);

            // Assert
            Assert.AreEqual(VehicleSignalState.Green, location.NorthSouthVehicle);
            Assert.AreEqual(VehicleSignalState.Red, location.EastWestVehicle);
            Assert.AreEqual(1, location.Rules.Count);
            Assert.AreEqual(rule, location.Rules[0]);
        }

        [TestMethod]
        public void Restore_NullSerial_Throws()
        {
            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                IntersectionLocation.Restore(
                    null!, "Paris and Cedar", 46.49, -80.99,
                    VehicleSignalState.Red, VehicleSignalState.Red,
                    PedestrianSignalState.DontWalk, PedestrianSignalState.DontWalk);
            });
        }

        [TestMethod]
        public void ChangePhase_InvalidEnumValue_Throws()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                location.ChangePhase((Approach)99, VehicleSignalState.Green);
            });
        }

        [TestMethod]
        public void ChangePhase_SameState_IsNoOp()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act — already Red, asking for Red again should just return.
            location.ChangePhase(Approach.NorthSouth, VehicleSignalState.Red);

            // Assert
            Assert.AreEqual(VehicleSignalState.Red, location.NorthSouthVehicle);
        }

        [TestMethod]
        public void SetPedestrian_InvalidEnumValue_Throws()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act and Assert
            Assert.ThrowsExactly<ArgumentException>(() =>
            {
                location.SetPedestrian(Approach.NorthSouth, (PedestrianSignalState)99);
            });
        }

        [TestMethod]
        public void SetPedestrian_SameState_IsNoOp()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act — already DontWalk, asking for DontWalk again should just return.
            location.SetPedestrian(Approach.NorthSouth, PedestrianSignalState.DontWalk);

            // Assert
            Assert.AreEqual(PedestrianSignalState.DontWalk, location.NorthSouthPedestrian);
        }

        [TestMethod]
        public void SetPedestrian_EastWestApproach_Works()
        {
            // Arrange — existing tests only ever exercise NorthSouth; this
            // covers the EastWest branch of the same method.
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act
            location.SetPedestrian(Approach.EastWest, PedestrianSignalState.Walk);

            // Assert
            Assert.AreEqual(PedestrianSignalState.Walk, location.EastWestPedestrian);
        }

        [TestMethod]
        public void AddRule_NullRule_Throws()
        {
            // Arrange
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);

            // Act and Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                location.AddRule(null!);
            });
        }

        [TestMethod]
        public void AddRule_SecondRuleForDifferentApproach_Succeeds()
        {
            // Arrange — the duplicate-approach check must only reject a
            // matching approach, not reject outright whenever any rule
            // already exists for the location.
            SerialNumber locationSerial = SerialNumber.Parse("INT-001");
            IntersectionLocation location = IntersectionLocation.Register(locationSerial);
            SignalRule northSouthRule = SignalRule.Create(
                locationSerial, Approach.NorthSouth, 10, 30, 3, 5, 8);
            SignalRule eastWestRule = SignalRule.Create(
                locationSerial, Approach.EastWest, 10, 30, 3, 5, 8);

            // Act
            location.AddRule(northSouthRule);
            location.AddRule(eastWestRule);

            // Assert
            Assert.AreEqual(2, location.Rules.Count);
            CollectionAssert.Contains(location.Rules.ToList(), northSouthRule);
            CollectionAssert.Contains(location.Rules.ToList(), eastWestRule);
        }
    }
}
