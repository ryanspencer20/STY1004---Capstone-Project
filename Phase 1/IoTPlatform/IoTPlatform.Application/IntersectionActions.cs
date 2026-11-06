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
        private readonly ISignalRuleRepository repositoryRule; 


        public IntersectionActions(
            IIntersectionDeviceRepository repositoryDevice,
            ITrafficTelemetryRepository repositoryTelemetry,
            IIntersectionLocationRepository repositoryLocation,
            ISignalRuleRepository repositoryRule)
        {
            this.repositoryDevice = repositoryDevice;
            this.repositoryTelemetry = repositoryTelemetry;
            this.repositoryLocation = repositoryLocation;
            this.repositoryRule = repositoryRule;
        }

        public IntersectionLocation TrafficSignalCorridorOverride(SerialNumber locationSerial, Approach approach, VehicleSignalState nextState,  SignalRule signalRule)
        {
            var location = repositoryLocation.GetLocationBySerial(locationSerial)
                ?? throw new InvalidOperationException("Location not found.");

            repositoryRule.Apply(location, signalRule);

            if (signalRule.IsEnabled)
            {
                location.ChangePhase(approach, nextState); // domain enforces the rule
                repositoryLocation.UpdateLocation(location);
            }
            return location; 
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