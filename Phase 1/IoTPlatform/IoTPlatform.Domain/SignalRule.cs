using System;

namespace IoTPlatform.Domain
{
    public class SignalRule
    {
        public SerialNumber DeviceSerial { get; private set; }
        public Approach Approach { get; private set; }
        public int MinGreenSeconds { get; private set; }
        public int MaxGreenSeconds { get; private set; }
        public int YellowSeconds { get; private set; }
        public int AdvancedGreenSeconds { get; private set; }
        public int CountOverrideThreshold { get; private set; }
        public bool IsEnabled { get; private set; }

        private SignalRule(
            SerialNumber device,
            Approach approach,
            int minGreen,
            int maxGreen,
            int yellow,
            int advancedGreen,
            int threshold,
            bool isEnabled)
        {
            DeviceSerial = device;
            Approach = approach;
            MinGreenSeconds = minGreen;
            MaxGreenSeconds = maxGreen;
            YellowSeconds = yellow;
            AdvancedGreenSeconds = advancedGreen;
            CountOverrideThreshold = threshold;
            IsEnabled = isEnabled;
        }

        public static SignalRule Create(
            SerialNumber device,
            Approach approach,
            int minGreen,
            int maxGreen,
            int yellow,
            int advancedGreen,
            int threshold,
            bool isEnabled = true)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (approach != Approach.NorthSouth &&
                approach != Approach.EastWest)
            {
                throw new ArgumentException("Invalid approach.");
            }

            if (minGreen <= 0 || maxGreen <= 0 ||
                yellow <= 0 || advancedGreen <= 0)
            {
                throw new ArgumentException(
                    "All durations must be greater than zero.");
            }

            if (minGreen > maxGreen)
            {
                throw new ArgumentException(
                    "Minimum green cannot exceed maximum green.");
            }

            if (threshold <= 0)
            {
                throw new ArgumentException(
                    "Vehicle count threshold must be greater than zero.");
            }

            return new SignalRule(
                device, approach, minGreen, maxGreen,
                yellow, advancedGreen, threshold, isEnabled);
        }

        public bool ShouldOverride(TrafficTelemetry telemetry)
        {
            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            if (!IsEnabled)
            {
                return false;
            }

            if (telemetry.DeviceSerial != DeviceSerial ||
                telemetry.Approach != Approach)
            {
                return false;
            }

            return telemetry.VehicleCount >= CountOverrideThreshold;
        }

        public VehicleSignalState NextState(
    VehicleSignalState current,
    int elapsed)
        {
            if (!Enum.IsDefined(typeof(VehicleSignalState), current))
            {
                throw new ArgumentException("Invalid signal state.");
            }

            if (elapsed < 0)
            {
                throw new ArgumentException(
                    "Elapsed time cannot be negative.");
            }

            if (!IsEnabled)
            {
                return current;
            }

            if (current == VehicleSignalState.AdvancedGreen &&
                elapsed >= AdvancedGreenSeconds)
            {
                return VehicleSignalState.Green;
            }

            if (current == VehicleSignalState.Green &&
                elapsed >= MaxGreenSeconds)
            {
                return VehicleSignalState.Yellow;
            }

            if (current == VehicleSignalState.Yellow &&
                elapsed >= YellowSeconds)
            {
                return VehicleSignalState.Red;
            }

            return current;
        }
    }
}