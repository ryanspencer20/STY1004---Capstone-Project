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

        public void ExecuteRegistration(SerialNumber serial, string name)
        {
            var device = IntersectionDevice.Register(serial, name);
            repository.SaveDevice(device);            
        }

        public IntersectionDevice GetDeviceBySerialNumber(SerialNumber serial)
        {
            var device = repository.GetDeviceBySerial(serial)
                ?? throw new InvalidOperationException("Device not found.");
            return device;
        }

        public void ExecuteAddRule(SerialNumber serial, SignalRule rule)
        {
            var device = repository.GetDeviceBySerial(serial)
                ?? throw new InvalidOperationException("Device not found.");
            device.AddRule(rule);
            repository.SaveDevice(device);
        }

        public void ExecuteUpdateDevice(IntersectionDevice device)
        {
            var existing = repository.GetDeviceBySerial(device.Serial)
                ?? throw new InvalidOperationException("Device not found.");
            repository.UpdateDevice(existing);
        }
    }
}