using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            signalRuleModel.HasKey(e => e.Intersection_SerialID);

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
            trafficTelemetryModel.HasKey(e => e.Recorded_At);

            trafficTelemetryModel
                .Property(e => e.Data)
                .IsRequired();

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
            violationLogModel.HasKey(e => e.Occured_At);

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
            public string Device_Serial { get; set; } = string.Empty; // Primary key for the Blog entity
            public string Location_SerialID { get; set; } = string.Empty;
            public string Device_Name { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string NorthSouthVehicle { get; set; } = string.Empty;
            public string EastWestVehicle { get; set; } = string.Empty;
            public string NorthSouthPedestrian { get; set; } = string.Empty;
            public string EastWestPedestrian { get; set; } = string.Empty;
            public List<Violation_log> Violation_logs { get; } = new();

            public List<Signal_Rule> Signal_Rules { get; } = new();
            public List<Traffic_Telemetry> Traffic_Telemetries { get; } = new();
        }

        public class Signal_Rule
        {
            public string Intersection_SerialID { get; set; } = string.Empty; // Primary key for the Signal_Rule entity
            public string Approach { get; set; } = string.Empty;
            public int Min_Green_Time_Sec { get; set; }
            public int Max_Green_Time_Sec { get; set; }
            public int Advanced_Green_Time_Sec { get; set; }
            public int Yellow_Time_Sec { get; set; }
            public string Device_Serial { get; set; } = string.Empty; // Foreign key for the IntersectionDevice entity
            public IntersectionDevice IntersectionDevice { get; set; } = null!;
        }
        public class Traffic_Telemetry
        {
            public DateTime Recorded_At { get; set; } = DateTime.UtcNow; // Primary key for the Traffic_Telemetry entity
            public string Data { get; set; } = string.Empty;
            public string Device_Serial { get; set; } = string.Empty; // Foreign key for the IntersectionDevice entity
            public string Vehicle_Count { get; set; } = string.Empty;
            public double Speed_Kph { get; set; }
            public IntersectionDevice IntersectionDevice { get; set; } = null!;
            
        }
        public class Violation_log
        {
            public DateTime Occured_At { get; set; } = DateTime.UtcNow; // Primary key for the Violation_log entity
            public string Violation_Type { get; set; } = string.Empty;
            public string Plate_Number { get; set; } = string.Empty;
            public double Speed_Kph { get; set; }
            public string Signal_At_Time { get; set; } = string.Empty;
            

            public string Device_Serial { get; set; } = string.Empty; // Foreign key for the IntersectionDevice entity
            public IntersectionDevice IntersectionDevice { get; set; } = null!;
        }
    }
}