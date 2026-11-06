using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface IIntersectionLocationRepository
    {
        public IntersectionLocation? GetLocationBySerial(SerialNumber serial);
        public void SaveLocation(IntersectionLocation location);
        public void UpdateLocation(IntersectionLocation location);
    }
}
