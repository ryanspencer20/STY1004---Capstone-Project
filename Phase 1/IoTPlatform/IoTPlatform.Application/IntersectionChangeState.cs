using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Application
{
    public class IntersectionChangeState
    {
        private readonly IIntersectionDeviceRepository repository;

        public IntersectionChangeState(IIntersectionDeviceRepository repository)
        {
            this.repository = repository;
        }

        public void Execute(SerialNumber serial, Approach approach, VehicleSignalState nextState)
        {
            var device = repository.GetDeviceBySerial(serial)
                ?? throw new InvalidOperationException("Device not found.");

            device.ChangePhase(approach, nextState); // domain enforces the rule
            repository.SaveDevice(device);
        }
    }
}