using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface IIntersectionDeviceRepository
    {
        public IntersectionDevice? GetDeviceBySerial(SerialNumber serial);
        public void SaveDevice(IntersectionDevice device);
        public void UpdateDevice(IntersectionDevice device);
    }
}