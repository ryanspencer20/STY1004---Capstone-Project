using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Application
{
    public class DeviceActions
    {
        private readonly IIntersectionDeviceRepository repository;

        public DeviceActions(IIntersectionDeviceRepository repository)
        {
            ArgumentNullException.ThrowIfNull(repository);
            this.repository = repository;
        }

        public void ExecuteRegistration(SerialNumber device, SerialNumber location, string name, FacingDirection facingDirection, string firmwareVersion)
        {
            var checkDevice = SerialNumber.Parse(device.Value);
            var checkLocation = SerialNumber.Parse(location.Value);
            var newDevice = IntersectionDevice.Register(checkDevice, checkLocation, name, facingDirection, firmwareVersion);
            repository.SaveDevice(newDevice);            
        }

        public IntersectionDevice GetDeviceBySerialNumber(SerialNumber serial)
        {
            var device = repository.GetDeviceBySerial(serial)
                ?? throw new InvalidOperationException("Device not found.");
            return device;
        }

        public void ExecuteUpdateDevice(IntersectionDevice device)
        {
            // GetDeviceBySerial only confirms the device exists; the new
            // field values live on the incoming device, so that's what
            // must be persisted.
            _ = repository.GetDeviceBySerial(device.Serial)
                ?? throw new InvalidOperationException("Device not found.");
            repository.UpdateDevice(device);
        }
    }
}