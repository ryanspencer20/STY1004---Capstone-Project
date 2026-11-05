using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Application
{
    public class IntersectionActions
    {
        private readonly IIntersectionDeviceRepository repositoryDevice;
        private readonly ITrafficTelemetryRepository repositoryTelemetry;
        private readonly IIntersectionLocationRepository repositoryLocation;

        public IntersectionActions(
            IIntersectionDeviceRepository repositoryDevice,
            ITrafficTelemetryRepository repositoryTelemetry,
            IIntersectionLocationRepository repositoryLocation)
        {
            this.repositoryDevice = repositoryDevice;
            this.repositoryTelemetry = repositoryTelemetry;
            this.repositoryLocation = repositoryLocation;
        }

        public void TrafficSignalCorridorOverride(SerialNumber locationSerial, Approach approach, VehicleSignalState nextState)
        {
            var location = repositoryLocation.GetLocationBySerial(locationSerial)
                ?? throw new InvalidOperationException("Location not found.");

            location.ChangePhase(approach, nextState); // domain enforces the rule
            repositoryLocation.UpdateLocation(location);
        }

        public void ExecuteAddRule(SerialNumber locationSerial, SignalRule rule)
        {
            var location = repositoryLocation.GetLocationBySerial(locationSerial)
                ?? throw new InvalidOperationException("Location not found.");

            location.AddRule(rule);
            repositoryLocation.UpdateLocation(location);
        }

        public TrafficTelemetry InsertRecord(SerialNumber device, Approach approach, int count, decimal speed)
        {
            var telemetry = TrafficTelemetry.Record(device, approach, count, speed);
            return repositoryTelemetry.SaveTelemetry(telemetry);
        }

        public TrafficTelemetry GetTrafficSignalById(string telemetryId)
        {
            var telemetry = repositoryTelemetry.GetTelemetryByTelemetryId(telemetryId)
                ?? throw new InvalidOperationException("Telemetry not found.");
            return telemetry;
        }
    }
}