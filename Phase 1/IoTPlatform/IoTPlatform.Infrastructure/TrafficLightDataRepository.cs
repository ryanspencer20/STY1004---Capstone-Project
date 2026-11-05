using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTPlatform.Domain;

namespace IoTPlatform.Infrastructure
{
    public class TrafficLightDataRepository : IIntersectionDeviceRepository, ITrafficTelemetryRepository, IViolationLogRepository, IIntersectionLocationRepository
    {
        public IntersectionDevice? GetDeviceBySerial(SerialNumber serial)
        {
            using var db = new Database();

            var row = db.IntersectionDevices.Find(serial.Value);

            if (row == null)
            {
                return null;
            }

            return IntersectionDevice.Restore(
                serial,
                SerialNumber.Parse(row.Location_Serial),
                row.Device_Name,
                row.Facing_Direction,
                row.Firmware_Version,
                row.Status,
                row.Added_At);
        }

        public void SaveDevice(IntersectionDevice device)
        {
            using var db = new Database();

            var location = db.Locations.Find(device.LocationSerial.Value);
            if (location == null)
            {
                location = new Database.IntersectionLocation { Location_Serial = device.LocationSerial.Value };
                db.Locations.Add(location);
            }

            db.IntersectionDevices.Add(new Database.IntersectionDevice
            {
                Device_Serial = device.Serial.Value,
                Location_Serial = device.LocationSerial.Value,
                Device_Name = device.DisplayName,
                Status = device.Status,
                Facing_Direction = device.FacingDirection,
                Firmware_Version = device.FirmwareVersion,
                Added_At = device.CommissionedDate,
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

            if (db.Locations.Find(device.LocationSerial.Value) == null)
            {
                db.Locations.Add(new Database.IntersectionLocation { Location_Serial = device.LocationSerial.Value });
            }

            existing.Device_Name = device.DisplayName;
            existing.Status = device.Status;
            existing.Facing_Direction = device.FacingDirection;
            existing.Firmware_Version = device.FirmwareVersion;
            existing.Location_Serial = device.LocationSerial.Value;

            db.SaveChanges();
        }
        public TrafficTelemetry SaveTelemetry(TrafficTelemetry reading)
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
            return reading;
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

        public TrafficTelemetry GetTelemetryByTelemetryId(string telemetryId)
        {
            using var db = new Database();

            var row = db.Traffic_Telemetries.Find(telemetryId);

            if (row == null)
            {
                throw new InvalidOperationException("Telemetry not found.");
            }

            var telemetry = TrafficTelemetry.Record(
                SerialNumber.Parse(row.Device_Serial),
                row.Approach,
                row.Vehicle_Count,
                row.Speed_Kph
            );
            return telemetry;
        }

        public IntersectionLocation? GetLocationBySerial(SerialNumber serial)
        {
            using var db = new Database();

            var row = db.Locations.Find(serial.Value);
            if (row == null)
            {
                return null;
            }

            var state = db.Signal_States.Find(serial.Value);

            var rules = db.Signal_Rules
                .Where(r => r.Location_Serial == serial.Value)
                .Select(r => SignalRule.Create(
                    serial,
                    r.Approach,
                    r.Min_Green_Time_Sec,
                    r.Max_Green_Time_Sec,
                    r.Yellow_Time_Sec,
                    r.Advanced_Green_Time_Sec,
                    r.CountOverrideThreshold,
                    r.IsEnabled))
                .ToList();

            return IntersectionLocation.Restore(
                serial,
                row.Location_Name,
                row.Latitude,
                row.Longitude,
                state?.NorthSouthVehicle ?? VehicleSignalState.Red,
                state?.EastWestVehicle ?? VehicleSignalState.Red,
                state?.NorthSouthPedestrian ?? PedestrianSignalState.DontWalk,
                state?.EastWestPedestrian ?? PedestrianSignalState.DontWalk,
                rules);
        }

        public void SaveLocation(IntersectionLocation location)
        {
            using var db = new Database();

            db.Locations.Add(new Database.IntersectionLocation
            {
                Location_Serial = location.LocationSerial.Value,
                Location_Name = location.LocationName,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
            });

            db.Signal_States.Add(new Database.Signal_State
            {
                Location_Serial = location.LocationSerial.Value,
                NorthSouthVehicle = location.NorthSouthVehicle,
                EastWestVehicle = location.EastWestVehicle,
                NorthSouthPedestrian = location.NorthSouthPedestrian,
                EastWestPedestrian = location.EastWestPedestrian,
            });

            foreach (SignalRule rule in location.Rules)
            {
                db.Signal_Rules.Add(new Database.Signal_Rule
                {
                    Location_Serial = location.LocationSerial.Value,
                    Approach = rule.Approach,
                    Min_Green_Time_Sec = rule.MinGreenSeconds,
                    Max_Green_Time_Sec = rule.MaxGreenSeconds,
                    Yellow_Time_Sec = rule.YellowSeconds,
                    Advanced_Green_Time_Sec = rule.AdvancedGreenSeconds,
                    CountOverrideThreshold = rule.CountOverrideThreshold,
                    IsEnabled = rule.IsEnabled,
                });
            }

            db.SaveChanges();
        }

        public void UpdateLocation(IntersectionLocation location)
        {
            using var db = new Database();

            var existing = db.Locations.Find(location.LocationSerial.Value)
                ?? throw new InvalidOperationException("Location not found.");

            existing.Location_Name = location.LocationName;
            existing.Latitude = location.Latitude;
            existing.Longitude = location.Longitude;

            var state = db.Signal_States.Find(location.LocationSerial.Value);
            if (state == null)
            {
                state = new Database.Signal_State { Location_Serial = location.LocationSerial.Value };
                db.Signal_States.Add(state);
            }
            state.NorthSouthVehicle = location.NorthSouthVehicle;
            state.EastWestVehicle = location.EastWestVehicle;
            state.NorthSouthPedestrian = location.NorthSouthPedestrian;
            state.EastWestPedestrian = location.EastWestPedestrian;

            var existingRules = db.Signal_Rules.Where(r => r.Location_Serial == location.LocationSerial.Value);
            db.Signal_Rules.RemoveRange(existingRules);

            foreach (SignalRule rule in location.Rules)
            {
                db.Signal_Rules.Add(new Database.Signal_Rule
                {
                    Location_Serial = location.LocationSerial.Value,
                    Approach = rule.Approach,
                    Min_Green_Time_Sec = rule.MinGreenSeconds,
                    Max_Green_Time_Sec = rule.MaxGreenSeconds,
                    Yellow_Time_Sec = rule.YellowSeconds,
                    Advanced_Green_Time_Sec = rule.AdvancedGreenSeconds,
                    CountOverrideThreshold = rule.CountOverrideThreshold,
                    IsEnabled = rule.IsEnabled,
                });
            }

            db.SaveChanges();
        }
    }
}
