using System;

namespace IoTPlatform.Domain
{
    public class ViolationLog
    {
        public SerialNumber DeviceSerial { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }
        public ViolationType Type { get; private set; }
        public string? Plate { get; private set; }
        public decimal? SpeedKph { get; private set; }
        public VehicleSignalState SignalAtTime { get; private set; }

        private ViolationLog(
            SerialNumber device,
            ViolationType type,
            string? plate,
            decimal? speed,
            VehicleSignalState signal)
        {
            DeviceSerial = device;
            OccurredAt = DateTimeOffset.UtcNow;
            Type = type;
            Plate = plate;
            SpeedKph = speed;
            SignalAtTime = signal;
        }

        public static ViolationLog LogRedLight(
            SerialNumber device,
            VehicleSignalState signal,
            string? plate = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (signal != VehicleSignalState.Red)
            {
                throw new ArgumentException(
                    "A red light violation must occur on red.",
                    nameof(signal));
            }

            return new ViolationLog(
                device, ViolationType.RedLight, plate, null, signal);
        }

        public static ViolationLog LogSpeeding(
            SerialNumber device,
            decimal speed,
            VehicleSignalState signal,
            string? plate = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (speed <= 0)
            {
                throw new ArgumentException(
                    "Speed must be greater than zero.",
                    nameof(speed));
            }

            if (!Enum.IsDefined(typeof(VehicleSignalState), signal))
            {
                throw new ArgumentException(
                    "Signal state must be valid.",
                    nameof(signal));
            }

            return new ViolationLog(
                device, ViolationType.Speeding, plate, speed, signal);
        }
    }
}

