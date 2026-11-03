using System;
using System.Collections.Generic;

namespace IoTPlatform.Domain
{
    public class IntersectionDevice
    {
        public SerialNumber Serial { get; private set; }
        public string DisplayName { get; private set; }
        public VehicleSignalState NorthSouthVehicle { get; private set; }
        public VehicleSignalState EastWestVehicle { get; private set; }
        public PedestrianSignalState NorthSouthPedestrian { get; private set; }
        public PedestrianSignalState EastWestPedestrian { get; private set; }
        public DeviceLifecycle Status { get; private set; }

        private readonly List<SignalRule> rules = new List<SignalRule>();

        public IReadOnlyList<SignalRule> Rules
        {
            get { return rules.AsReadOnly(); }
        }

        private IntersectionDevice(SerialNumber serial, string name)
        {
            Serial = serial;
            DisplayName = name;
            Status = DeviceLifecycle.Commissioned;

            NorthSouthVehicle = VehicleSignalState.Red;
            EastWestVehicle = VehicleSignalState.Red;
            NorthSouthPedestrian = PedestrianSignalState.DontWalk;
            EastWestPedestrian = PedestrianSignalState.DontWalk;
        }

        public static IntersectionDevice Register(
            SerialNumber serial, string name)
        {
            if (serial == null)
            {
                throw new ArgumentNullException(nameof(serial));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Device name is required.");
            }

            return new IntersectionDevice(serial, name);
        }
        public void Activate()
        {
            if (Status != DeviceLifecycle.Commissioned)
            {
                throw new InvalidOperationException(
                    "Only a commissioned device can be activated.");
            }

            Status = DeviceLifecycle.Active;
        }

        public void Deactivate()
        {
            if (Status != DeviceLifecycle.Active &&
                Status != DeviceLifecycle.Degraded)
            {
                throw new InvalidOperationException(
                    "Only an active or degraded device can be deactivated.");
            }

            NorthSouthVehicle = VehicleSignalState.Red;
            EastWestVehicle = VehicleSignalState.Red;
            NorthSouthPedestrian = PedestrianSignalState.DontWalk;
            EastWestPedestrian = PedestrianSignalState.DontWalk;

            Status = DeviceLifecycle.Decommissioned;
        }

        public void ChangePhase(
            Approach approach, VehicleSignalState nextState)
        {
            if (Status != DeviceLifecycle.Active)
            {
                throw new InvalidOperationException(
                    "Device must be active.");
            }

            if (!Enum.IsDefined(typeof(Approach), approach) ||
                !Enum.IsDefined(typeof(VehicleSignalState), nextState))
            {
                throw new ArgumentException(
                    "Invalid approach or signal state.");
            }

            bool northSouth = approach == Approach.NorthSouth;

            VehicleSignalState current = northSouth
                ? NorthSouthVehicle : EastWestVehicle;

            VehicleSignalState opposite = northSouth
                ? EastWestVehicle : NorthSouthVehicle;

            if (current == nextState)
            {
                return;
            }

            bool allowed =
                (current == VehicleSignalState.Red &&
                    (nextState == VehicleSignalState.Green ||
                     nextState == VehicleSignalState.AdvancedGreen)) ||
                (current == VehicleSignalState.AdvancedGreen &&
                    nextState == VehicleSignalState.Green) ||
                (current == VehicleSignalState.Green &&
                    nextState == VehicleSignalState.Yellow) ||
                (current == VehicleSignalState.Yellow &&
                    nextState == VehicleSignalState.Red);

            if (!allowed)
            {
                throw new InvalidOperationException(
                    "Invalid signal transition.");
            }

            if (nextState == VehicleSignalState.Green ||
                nextState == VehicleSignalState.AdvancedGreen)
            {
                if (opposite != VehicleSignalState.Red ||
                    NorthSouthPedestrian != PedestrianSignalState.DontWalk ||
                    EastWestPedestrian != PedestrianSignalState.DontWalk)
                {
                    throw new InvalidOperationException(
                        "Other traffic and pedestrians must be stopped first.");
                }
            }

            if (northSouth)
            {
                NorthSouthVehicle = nextState;
            }
            else
            {
                EastWestVehicle = nextState;
            }
        }

        public void StartAdvancedGreen(Approach approach)
        {
            ChangePhase(approach, VehicleSignalState.AdvancedGreen);
        }

        public void SetPedestrian(
            Approach approach, PedestrianSignalState nextState)
        {
            if (Status != DeviceLifecycle.Active)
            {
                throw new InvalidOperationException(
                    "Device must be active.");
            }

            if (!Enum.IsDefined(typeof(Approach), approach) ||
                !Enum.IsDefined(typeof(PedestrianSignalState), nextState))
            {
                throw new ArgumentException(
                    "Invalid approach or pedestrian state.");
            }

            PedestrianSignalState current;

            if (approach == Approach.NorthSouth)
            {
                current = NorthSouthPedestrian;
            }
            else
            {
                current = EastWestPedestrian;
            }

            if (current == nextState)
            {
                return;
            }

            bool allowed =
                (current == PedestrianSignalState.DontWalk &&
                    nextState == PedestrianSignalState.Walk) ||
                (current == PedestrianSignalState.Walk &&
                    nextState == PedestrianSignalState.FlashingDontWalk) ||
                (current == PedestrianSignalState.FlashingDontWalk &&
                    nextState == PedestrianSignalState.DontWalk);

            if (!allowed)
            {
                throw new InvalidOperationException(
                    "Invalid pedestrian transition.");
            }

            if (nextState != PedestrianSignalState.DontWalk &&
                (NorthSouthVehicle != VehicleSignalState.Red ||
                 EastWestVehicle != VehicleSignalState.Red))
            {
                throw new InvalidOperationException(
                    "Both vehicle signals must be red for pedestrians.");
            }

            if (approach == Approach.NorthSouth)
            {
                NorthSouthPedestrian = nextState;
            }
            else
            {
                EastWestPedestrian = nextState;
            }
        }

        public void AddRule(SignalRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (rule.DeviceSerial != Serial)
            {
                throw new ArgumentException(
                    "Rule must belong to this device.");
            }

            foreach (SignalRule existing in rules)
            {
                if (existing.Approach == rule.Approach)
                {
                    throw new InvalidOperationException(
                        "A rule already exists for this approach.");
                }
            }

            rules.Add(rule);
        }
    }
}