using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Application
{
    public class ViolationAlerts
    {
        private readonly IViolationLogRepository repository;
        public ViolationAlerts(IViolationLogRepository repository)
        {
            this.repository = repository;
        }
        public ViolationLog? EvaluateIsRedLightViolation(SerialNumber device, VehicleSignalState signal, decimal? speed = null, string? plate = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }
            if (signal != VehicleSignalState.Red) return null;
            var redLightViolation = ViolationLog.LogRedLight(device, signal, speed, plate);
            if (redLightViolation != null)
            {
                repository.SaveViolation(redLightViolation);
                return redLightViolation;
            }
            return null;
        }

        public ViolationLog? EvaluateIsSpeedingViolation(SerialNumber device, VehicleSignalState signal, decimal? speed, string? plate = null)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }
            if (speed == null || speed <= 0) return null;
            if (speed > 70) {
                var speedingViolation = ViolationLog.LogSpeeding(device, signal, speed, plate);
                if (speedingViolation != null)
                {
                    repository.SaveViolation(speedingViolation);
                    return speedingViolation;
                }
            }
            return null;
        }
    }
}