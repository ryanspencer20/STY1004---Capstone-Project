using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IoTPlatform.Domain;

namespace IoTPlatform.Infrastructure
{
    public class Database : DbContext
    {
        public DbSet<IntersectionDevice> IntersectionDevices { get; set; }
        public DbSet<Signal_Rule> Signal_Rules { get; set; }
        public DbSet<Traffic_Telemetry> Traffic_Telemetries { get; set; }
        public DbSet<Violation_log> Violation_logs { get; set; }

        public Database()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = System.IO.Path.Join(path, "telemetry.db");
        }

        public string DbPath { get; }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var intersectionDeviceModel = modelBuilder.Entity<IntersectionDevice>();
            var signalRuleModel = modelBuilder.Entity<Signal_Rule>();
            var trafficTelemetryModel = modelBuilder.Entity<Traffic_Telemetry>();
            var violationLogModel = modelBuilder.Entity<Violation_log>();

            #region "IntersectionDevice Model Configuration"

            intersectionDeviceModel.HasKey(e => e.Device_Serial);

            intersectionDeviceModel
                .HasMany(e => e.Violation_logs)
                .WithOne(e => e.IntersectionDevice)
                .OnDelete(DeleteBehavior.Restrict)
                .HasForeignKey(e => e.Device_Serial);

            intersectionDeviceModel
                .HasMany(e => e.Signal_Rules)
                .WithOne(e => e.IntersectionDevice)
                .OnDelete(DeleteBehavior.Restrict)
                .HasForeignKey(e => e.Device_Serial);

            intersectionDeviceModel
                .HasMany(e => e.Traffic_Telemetries)
                .WithOne(e => e.IntersectionDevice)
                .OnDelete(DeleteBehavior.Restrict)
                .HasForeignKey(e => e.Device_Serial);

            intersectionDeviceModel
                .Property(e => e.Location_SerialID)
                .IsRequired();

            intersectionDeviceModel
                .Property(e => e.Device_Serial)
                .IsRequired();

            intersectionDeviceModel
                .Property(e => e.Device_Name)
                .IsRequired();
            #endregion

            #region "Signal_Rule Model Configuration"
            signalRuleModel.HasKey(e => new { e.Intersection_SerialID, e.Approach });

            signalRuleModel
                .Property(e => e.Yellow_Time_Sec)
                .IsRequired();

            signalRuleModel
                .Property(e => e.Min_Green_Time_Sec)
                .IsRequired();

            signalRuleModel
                .Property(e => e.Max_Green_Time_Sec)
                .IsRequired();

            signalRuleModel
                .Property(e => e.Advanced_Green_Time_Sec)
                .IsRequired();
            #endregion

            #region "Traffic_Telemetry Model Configuration"
            trafficTelemetryModel.HasKey(e => new { e.Device_Serial, e.Recorded_At, e.Approach });

            trafficTelemetryModel
                .Property(e => e.Device_Serial)
                .IsRequired();

            trafficTelemetryModel
                .Property(e => e.Vehicle_Count)
                .IsRequired();

            trafficTelemetryModel
                .Property(e => e.Speed_Kph)
                .IsRequired();
            #endregion

            #region "Violation_log Model Configuration"
            violationLogModel.HasKey(e => new { e.Device_Serial, e.Occured_At });

            violationLogModel
                .Property(e => e.Violation_Type)
                .IsRequired();

            violationLogModel
                .Property(e => e.Signal_At_Time)
                .IsRequired();
            #endregion
        }

        public class IntersectionDevice
        {
            public string Device_Serial { get; set; } = null!; // Primary key for the IntersectionDevice entity
            public string Location_SerialID { get; set; } = string.Empty;
            public string Device_Name { get; set; } = string.Empty;
            public DeviceLifecycle Status { get; set; } = new DeviceLifecycle();
            public VehicleSignalState NorthSouthVehicle { get; set; } = new VehicleSignalState();
            public VehicleSignalState EastWestVehicle { get; set; } = new VehicleSignalState();
            public PedestrianSignalState NorthSouthPedestrian { get; set; } = new PedestrianSignalState();
            public PedestrianSignalState EastWestPedestrian { get; set; } = new PedestrianSignalState();
            public List<Violation_log> Violation_logs { get; } = new();

            public List<Signal_Rule> Signal_Rules { get; } = new();
            public List<Traffic_Telemetry> Traffic_Telemetries { get; } = new();
        }

        public class Signal_Rule
        {
            public string Intersection_SerialID { get; set; } = string.Empty; // Primary key for the Signal_Rule entity
            public Approach Approach { get; set; } = new Approach();
            public int Min_Green_Time_Sec { get; set; }
            public int Max_Green_Time_Sec { get; set; }
            public int Advanced_Green_Time_Sec { get; set; }
            public int Yellow_Time_Sec { get; set; }
            public int CountOverrideThreshold { get; set; }
            public bool IsEnabled { get; set; }

            public string Device_Serial { get; set; } = string.Empty; // Foreign key for the IntersectionDevice entity
            public IntersectionDevice IntersectionDevice { get; set; } = null!;
        }
        public class Traffic_Telemetry
        {
            public string Device_Serial { get; set; } = null!; // Compound key for the Traffic_Telemetry entity and Foreign key for the IntersectionDevice entity
            public DateTime Recorded_At { get; set; } = DateTime.UtcNow; // Compound key for the Traffic_Telemetry entity

            public Approach Approach { get; set; } = new Approach();
            public int Vehicle_Count { get; set; } = 0;
            public decimal Speed_Kph { get; set; }

            public IntersectionDevice IntersectionDevice { get; set; } = null!;
            
        }
        public class Violation_log
        {
            public string Device_Serial { get; set; } = null!; // Primary and Foreign key for the IntersectionDevice entity
            public DateTime Occured_At { get; set; } = DateTime.UtcNow; // Compound key for the Violation_log entity
            public ViolationType Violation_Type { get; set; } = new ViolationType();
            public string? Plate_Number { get; set; } = string.Empty;
            public decimal? Speed_Kph { get; set; }
            public VehicleSignalState Signal_At_Time { get; set; } = new VehicleSignalState();
            
            public IntersectionDevice IntersectionDevice { get; set; } = null!;
        }
    }
}