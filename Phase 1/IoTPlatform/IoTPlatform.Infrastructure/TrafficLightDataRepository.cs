using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Infrastructure
{
    public class TrafficLightDataRepository : IIntersectionDeviceRepository, ITrafficTelemetryRepository, IViolationLogRepository
    {
        public IntersectionDevice? GetDeviceBySerial(SerialNumber serial)
        {
            using var db = new Database();

            var row = db.IntersectionDevices.Find(serial.Value);

            if (row == null)
            {
                return null;
            }
            return IntersectionDevice.Register(serial, row.Device_Name);
        }

        public void SaveDevice(IntersectionDevice device)
        {
            using var db = new Database();

            db.IntersectionDevices.Add(new Database.IntersectionDevice
            {
                Device_Serial = device.Serial.Value,
                Device_Name = device.DisplayName,
                Status = device.Status,
                NorthSouthVehicle = device.NorthSouthVehicle,
                EastWestVehicle = device.EastWestVehicle,
                NorthSouthPedestrian = device.NorthSouthPedestrian,
                EastWestPedestrian = device.EastWestPedestrian
            });

            db.SaveChanges();
        }

        public void UpdateDevice(IntersectionDevice device)
        {
            using var db = new Database();

            var existing = db.IntersectionDevices.Find(device.Serial.Value);
            if (existing == null)
            {
                throw new InvalidOperationException("Device not found.");
            }

            existing.Device_Name = device.DisplayName;
            existing.Status = device.Status;
            existing.NorthSouthVehicle = device.NorthSouthVehicle;
            existing.EastWestVehicle = device.EastWestVehicle;
            existing.NorthSouthPedestrian = device.NorthSouthPedestrian;
            existing.EastWestPedestrian = device.EastWestPedestrian;

            db.SaveChanges();
        }
        public void SaveTelemetry(TrafficTelemetry reading)
        {
            using var db = new Database();

            db.Traffic_Telemetries.Add(new Database.Traffic_Telemetry
            {
                Device_Serial = reading.DeviceSerial.Value,
                Recorded_At = reading.RecordedAt.UtcDateTime,
                Approach = reading.Approach,
                Vehicle_Count = reading.VehicleCount,
                Speed_Kph = reading.SpeedKph
            });

            db.SaveChanges();
        }

        public void SaveViolation(ViolationLog violation)
        {
            using var db = new Database();

            db.Violation_logs.Add(new Database.Violation_log
            {
                Device_Serial = violation.DeviceSerial.Value,
                Occured_At = violation.OccurredAt.UtcDateTime,
                Violation_Type = violation.Type,
                Plate_Number = violation.Plate,
                Speed_Kph = violation.SpeedKph,
                Signal_At_Time = violation.SignalAtTime
            });

            db.SaveChanges();
        }
    }
}
