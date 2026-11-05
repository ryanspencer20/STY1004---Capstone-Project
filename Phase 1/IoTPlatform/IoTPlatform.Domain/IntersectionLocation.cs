using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public class IntersectionLocation
    {
        public SerialNumber LocationSerial { get; private set; }
        public string LocationName { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public VehicleSignalState NorthSouthVehicle { get; private set; }
        public VehicleSignalState EastWestVehicle { get; private set; }
        public PedestrianSignalState NorthSouthPedestrian { get; private set; }
        public PedestrianSignalState EastWestPedestrian { get; private set; }      
        public DateTime SignalStateTimestamp { get; private set; }

        private readonly List<SignalRule> rules = new List<SignalRule>();

        public IReadOnlyList<SignalRule> Rules
        {
            get { return rules.AsReadOnly(); }
        }

        private IntersectionLocation(
            SerialNumber locationSerial,
            string locationName,
            double latitude,
            double longitude,
            VehicleSignalState northSouthVehicle,
            VehicleSignalState eastWestVehicle,
            PedestrianSignalState northSouthPedestrian,
            PedestrianSignalState eastWestPedestrian,
            DateTime signalStateTimestamp)
        {
            LocationSerial = locationSerial;
            LocationName = locationName;
            Latitude = latitude;
            Longitude = longitude;
            NorthSouthVehicle = northSouthVehicle;
            EastWestVehicle = eastWestVehicle;
            NorthSouthPedestrian = northSouthPedestrian;
            EastWestPedestrian = eastWestPedestrian;
            SignalStateTimestamp = signalStateTimestamp;
        }

        public static IntersectionLocation Register(
            SerialNumber locationSerial,
            string locationName = "",
            double latitude = 0.0,
            double longitude = 0.0)
        {
            if (locationSerial == null)
            {
                throw new ArgumentNullException(nameof(locationSerial));
            }

            return new IntersectionLocation(
                locationSerial,
                locationName ?? string.Empty,
                latitude,
                longitude,
                VehicleSignalState.Red,
                VehicleSignalState.Red,
                PedestrianSignalState.DontWalk,
                PedestrianSignalState.DontWalk,
                DateTime.UtcNow);
        }

        /// <summary>
        /// Rehydrates an existing location (with its current signal state
        /// and rules) from persistence, as opposed to Register which always
        /// starts a brand-new location in its default safe state.
        /// </summary>
        public static IntersectionLocation Restore(
            SerialNumber locationSerial,
            string locationName,
            double latitude,
            double longitude,
            VehicleSignalState northSouthVehicle,
            VehicleSignalState eastWestVehicle,
            PedestrianSignalState northSouthPedestrian,
            PedestrianSignalState eastWestPedestrian,
            IEnumerable<SignalRule>? rules = null,
            DateTime signalStateTimestamp = default)
        {
            if (locationSerial == null)
            {
                throw new ArgumentNullException(nameof(locationSerial));
            }

            var location = new IntersectionLocation(
                locationSerial,
                locationName ?? string.Empty,
                latitude,
                longitude,
                northSouthVehicle,
                eastWestVehicle,
                northSouthPedestrian,
                eastWestPedestrian,
                signalStateTimestamp == default ? DateTime.UtcNow : signalStateTimestamp);

            if (rules != null)
            {
                foreach (SignalRule rule in rules)
                {
                    location.AddRule(rule);
                }
            }

            return location;
        }

        public void ChangePhase(
            Approach approach, VehicleSignalState nextState)
        {
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

            if (rule.LocationSerial != LocationSerial)
            {
                throw new ArgumentException(
                    "Rule must belong to this location.");
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