using System;

namespace IoTPlatform.Domain
{
    public class TrafficTelemetry
    {
        public SerialNumber DeviceSerial { get; private set; }
        public DateTimeOffset RecordedAt { get; private set; }
        public Approach Approach { get; private set; }
        public int VehicleCount { get; private set; }
        public decimal SpeedKph { get; private set; }

        private TrafficTelemetry(
            SerialNumber device,
            Approach approach,
            int count,
            decimal speed)
        {
            DeviceSerial = device;
            RecordedAt = DateTimeOffset.UtcNow;
            Approach = approach;
            VehicleCount = count;
            SpeedKph = speed;
        }

        public static TrafficTelemetry Record(
            SerialNumber device,
            Approach approach,
            int count,
            decimal speed)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (approach != Approach.NorthSouth && approach != Approach.EastWest)
            {
                throw new ArgumentException(
                    "Approach must be NorthSouth or EastWest.",
                    nameof(approach));
            }

            if (count < 0)
            {
                throw new ArgumentException(
                    "Vehicle count cannot be negative.",
                    nameof(count));
            }

            if (speed <= 0)
            {
                throw new ArgumentException(
                    "Speed must be greater than zero.",
                    nameof(speed));
            }

            return new TrafficTelemetry(device, approach, count, speed);
        }

    }
}
