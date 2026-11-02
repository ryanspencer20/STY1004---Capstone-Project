using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface IIntersectionDeviceRepository
    {
        IntersectionDevice? GetDeviceBySerial(SerialNumber serial);
        void SaveDevice(IntersectionDevice device);
    }
}