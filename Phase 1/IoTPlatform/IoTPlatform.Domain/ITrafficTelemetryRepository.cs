using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTPlatform.Domain
{
    public interface ITrafficTelemetryRepository
    {
        public TrafficTelemetry SaveTelemetry(TrafficTelemetry reading);
        public TrafficTelemetry? GetTelemetryByTelemetryId(string telemetryId);
    }
}