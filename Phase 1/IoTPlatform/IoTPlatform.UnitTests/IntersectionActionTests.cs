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
    public class IntersectionActionTests
    {
        [TestMethod]
        public void TestTrafficSignalCorridorOverride()
        {
            // Arrange
            var locationSerial = SerialNumber.Parse("INT-001");
            var approach = Approach.NorthSouth;
            var nextState = VehicleSignalState.Green;
            var signalRule = SignalRule.Create(locationSerial, approach, 10, 30, 5, 5, 3, true);

            var repositoryDevice = new StubIntersectionDeviceRepository();
            var repositoryTelemetry = new SpyTrafficTelemetryRepository();
            var repositoryLocation = new SpyIntersectionLocationRepository(IntersectionLocation.Register(locationSerial));
            var repositoryRule = new SpySignalRuleRepository();

            var actions = new IntersectionActions(
                repositoryDevice,
                repositoryTelemetry,
                repositoryLocation,
                repositoryRule);

            // Act
            var result = actions.TrafficSignalCorridorOverride(locationSerial, approach, nextState, signalRule);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(VehicleSignalState.Green, result.NorthSouthVehicle);
            Assert.AreSame(signalRule, repositoryRule.AppliedRule);
            Assert.AreSame(result, repositoryLocation.UpdatedLocation);
        }

        [TestMethod]
        public void TrafficSignalCorridorOverride_LocationNotFound_Throws()
        {
            // Arrange
            var locationSerial = SerialNumber.Parse("INT-001");
            var approach = Approach.NorthSouth;
            var signalRule = SignalRule.Create(locationSerial, approach, 10, 30, 5, 5, 3, true);

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                new SpyTrafficTelemetryRepository(),
                new SpyIntersectionLocationRepository(location: null),
                new SpySignalRuleRepository());

            // Act
            Action act = () => actions.TrafficSignalCorridorOverride(
                locationSerial, approach, VehicleSignalState.Green, signalRule);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void TrafficSignalCorridorOverride_DisabledRule_AppliesRuleButSkipsPhaseChange()
        {
            // Arrange — a disabled rule should still be persisted via Apply,
            // but ChangePhase/UpdateLocation must NOT run (the override is off).
            var locationSerial = SerialNumber.Parse("INT-001");
            var approach = Approach.NorthSouth;
            var signalRule = SignalRule.Create(locationSerial, approach, 10, 30, 5, 5, 3, isEnabled: false);

            var repositoryLocation = new SpyIntersectionLocationRepository(IntersectionLocation.Register(locationSerial));
            var repositoryRule = new SpySignalRuleRepository();

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                new SpyTrafficTelemetryRepository(),
                repositoryLocation,
                repositoryRule);

            // Act
            var result = actions.TrafficSignalCorridorOverride(
                locationSerial, approach, VehicleSignalState.Green, signalRule);

            // Assert
            Assert.AreEqual(VehicleSignalState.Red, result.NorthSouthVehicle); // unchanged
            Assert.AreSame(signalRule, repositoryRule.AppliedRule); // rule still applied
            Assert.IsNull(repositoryLocation.UpdatedLocation); // but never persisted as a phase change
        }

        [TestMethod]
        public void ExecuteAddRule_LocationFound_AddsRuleAndPersistsLocation()
        {
            // Arrange
            var locationSerial = SerialNumber.Parse("INT-001");
            var rule = SignalRule.Create(locationSerial, Approach.EastWest, 10, 30, 5, 5, 3, true);
            var repositoryLocation = new SpyIntersectionLocationRepository(IntersectionLocation.Register(locationSerial));

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                new SpyTrafficTelemetryRepository(),
                repositoryLocation,
                new SpySignalRuleRepository());

            // Act
            actions.ExecuteAddRule(locationSerial, rule);

            // Assert
            Assert.IsNotNull(repositoryLocation.UpdatedLocation);
            CollectionAssert.Contains(repositoryLocation.UpdatedLocation!.Rules.ToList(), rule);
        }

        [TestMethod]
        public void ExecuteAddRule_LocationNotFound_Throws()
        {
            // Arrange
            var locationSerial = SerialNumber.Parse("INT-001");
            var rule = SignalRule.Create(locationSerial, Approach.EastWest, 10, 30, 5, 5, 3, true);

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                new SpyTrafficTelemetryRepository(),
                new SpyIntersectionLocationRepository(location: null),
                new SpySignalRuleRepository());

            // Act
            Action act = () => actions.ExecuteAddRule(locationSerial, rule);

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        [TestMethod]
        public void InsertRecord_SavesTelemetryAndReturnsSavedReading()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var repositoryTelemetry = new SpyTrafficTelemetryRepository();

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                repositoryTelemetry,
                new SpyIntersectionLocationRepository(location: null),
                new SpySignalRuleRepository());

            // Act
            var result = actions.InsertRecord(device, Approach.NorthSouth, 7, 45.5m);

            // Assert
            Assert.IsNotNull(repositoryTelemetry.SavedTelemetry);
            Assert.AreEqual(device, repositoryTelemetry.SavedTelemetry!.DeviceSerial);
            Assert.AreEqual(7, repositoryTelemetry.SavedTelemetry.VehicleCount);
            Assert.AreEqual(45.5m, repositoryTelemetry.SavedTelemetry.SpeedKph);
            Assert.AreSame(repositoryTelemetry.SavedTelemetry, result);
        }

        [TestMethod]
        public void GetTrafficSignalById_Found_ReturnsTelemetry()
        {
            // Arrange
            var device = SerialNumber.Parse("CAM-001");
            var expected = TrafficTelemetry.Record(device, Approach.NorthSouth, 4, 50m);
            var repositoryTelemetry = new SpyTrafficTelemetryRepository { TelemetryToReturn = expected };

            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                repositoryTelemetry,
                new SpyIntersectionLocationRepository(location: null),
                new SpySignalRuleRepository());

            // Act
            var result = actions.GetTrafficSignalById("any-id");

            // Assert
            Assert.AreSame(expected, result);
        }

        [TestMethod]
        public void GetTrafficSignalById_NotFound_Throws()
        {
            // Arrange
            var actions = new IntersectionActions(
                new StubIntersectionDeviceRepository(),
                new SpyTrafficTelemetryRepository(),
                new SpyIntersectionLocationRepository(location: null),
                new SpySignalRuleRepository());

            // Act
            Action act = () => actions.GetTrafficSignalById("missing-id");

            // Assert
            Assert.ThrowsExactly<InvalidOperationException>(act);
        }

        /// <summary>
        /// Minimal test doubles implementing the repository interfaces
        /// directly (matching the pattern in DeviceActionsTests), so tests
        /// stay isolated without depending on a mocking library.
        /// </summary>
        private sealed class StubIntersectionDeviceRepository : IIntersectionDeviceRepository
        {
            public IntersectionDevice? GetDeviceBySerial(SerialNumber serial) => null;
            public void SaveDevice(IntersectionDevice device) { }
            public void UpdateDevice(IntersectionDevice device) { }
        }

        /// <summary>
        /// Records whatever gets saved and lets a test configure what a
        /// lookup should return (including null, for "not found" cases).
        /// </summary>
        private sealed class SpyTrafficTelemetryRepository : ITrafficTelemetryRepository
        {
            public TrafficTelemetry? TelemetryToReturn { get; set; }
            public TrafficTelemetry? SavedTelemetry { get; private set; }

            public TrafficTelemetry SaveTelemetry(TrafficTelemetry reading)
            {
                SavedTelemetry = reading;
                return reading;
            }

            public TrafficTelemetry? GetTelemetryByTelemetryId(string telemetryId) => TelemetryToReturn;
            public TrafficTelemetry? GetLastTelemetryByDevice(SerialNumber device) => null;
        }

        private sealed class SpyIntersectionLocationRepository : IIntersectionLocationRepository
        {
            private readonly IntersectionLocation? location;

            public SpyIntersectionLocationRepository(IntersectionLocation? location)
            {
                this.location = location;
            }

            public IntersectionLocation? UpdatedLocation { get; private set; }

            public IntersectionLocation? GetLocationBySerial(SerialNumber serial) => location;
            public void SaveLocation(IntersectionLocation location) { }
            public void UpdateLocation(IntersectionLocation location)
            {
                UpdatedLocation = location;
            }
        }

        private sealed class SpySignalRuleRepository : ISignalRuleRepository
        {
            public SignalRule? AppliedRule { get; private set; }
            public IntersectionLocation? AppliedLocation { get; private set; }

            public void Add(SignalRule rule) { }

            public void Apply(IntersectionLocation location, SignalRule rule)
            {
                AppliedLocation = location;
                AppliedRule = rule;
            }

            public IReadOnlyList<SignalRule> GetAll() => new List<SignalRule>();
        }
    }
}