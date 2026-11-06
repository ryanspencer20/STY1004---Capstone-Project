using System;
using System.Collections.Generic;

namespace IoTPlatform.Domain
{
    public class IntersectionDevice
    {
        public SerialNumber Serial { get; private set; }
        public SerialNumber LocationSerial { get; private set; }
        public string DisplayName { get; private set; }
        public SerialNumber Location { get; private set; }
        public FacingDirection FacingDirection { get; private set; }
        public string FirmwareVersion { get; private set; }
        public DateTime CommissionedDate { get; private set; }


        public DeviceLifecycle Status { get; private set; }

        private IntersectionDevice(
            SerialNumber serial,
            SerialNumber location,
            string name,
            FacingDirection facingDirection,
            string firmwareVersion,
            DeviceLifecycle status,
            DateTime commissionedDate)
        {
            Serial = serial;
            Location = location;
            LocationSerial = location;
            DisplayName = name;
            FacingDirection = facingDirection;
            Status = status;
            CommissionedDate = commissionedDate;
            FirmwareVersion = firmwareVersion;
        }

        public static IntersectionDevice Register(
            SerialNumber serial, SerialNumber location, string name, FacingDirection facingDirection, string firmwareVersion)
        {
            ValidateRegistrationInputs(serial, location, name, firmwareVersion);

            return new IntersectionDevice(
                serial, location, name, facingDirection, firmwareVersion,
                DeviceLifecycle.Commissioned, DateTime.UtcNow);
        }

        /// <summary>
        /// Rehydrates an existing device (with its real lifecycle status and
        /// commissioned date) from persistence, as opposed to Register which
        /// always starts a brand-new device as Commissioned.
        /// </summary>
        public static IntersectionDevice Restore(
            SerialNumber serial, SerialNumber location, string name, FacingDirection facingDirection,
            string firmwareVersion, DeviceLifecycle status, DateTime commissionedDate)
        {
            ValidateRegistrationInputs(serial, location, name, firmwareVersion);

            return new IntersectionDevice(
                serial, location, name, facingDirection, firmwareVersion,
                status, commissionedDate);
        }

        private static void ValidateRegistrationInputs(
            SerialNumber serial, SerialNumber location, string name, string firmwareVersion)
        {
            if (serial == null)
            {
                throw new ArgumentNullException(nameof(serial));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Device name is required.");
            }

            if (string.IsNullOrWhiteSpace(firmwareVersion))
            {
                throw new ArgumentException("Firmware version is required.");
            }
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
            Status = DeviceLifecycle.Decommissioned;
        }
    }
}