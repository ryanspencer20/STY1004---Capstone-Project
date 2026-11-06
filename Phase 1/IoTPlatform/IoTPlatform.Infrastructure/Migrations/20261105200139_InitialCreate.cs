using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Location_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Location_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Location_Serial);
                });

            migrationBuilder.CreateTable(
                name: "IntersectionDevices",
                columns: table => new
                {
                    Device_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Device_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Facing_Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    Added_At = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Firmware_Version = table.Column<string>(type: "TEXT", nullable: false),
                    Location_Serial = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntersectionDevices", x => x.Device_Serial);
                    table.ForeignKey(
                        name: "FK_IntersectionDevices_Locations_Location_Serial",
                        column: x => x.Location_Serial,
                        principalTable: "Locations",
                        principalColumn: "Location_Serial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Signal_Rules",
                columns: table => new
                {
                    Location_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Approach = table.Column<int>(type: "INTEGER", nullable: false),
                    Min_Green_Time_Sec = table.Column<int>(type: "INTEGER", nullable: false),
                    Max_Green_Time_Sec = table.Column<int>(type: "INTEGER", nullable: false),
                    Advanced_Green_Time_Sec = table.Column<int>(type: "INTEGER", nullable: false),
                    Yellow_Time_Sec = table.Column<int>(type: "INTEGER", nullable: false),
                    CountOverrideThreshold = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signal_Rules", x => new { x.Location_Serial, x.Approach });
                    table.ForeignKey(
                        name: "FK_Signal_Rules_Locations_Location_Serial",
                        column: x => x.Location_Serial,
                        principalTable: "Locations",
                        principalColumn: "Location_Serial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Signal_States",
                columns: table => new
                {
                    Location_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NorthSouthVehicle = table.Column<int>(type: "INTEGER", nullable: false),
                    EastWestVehicle = table.Column<int>(type: "INTEGER", nullable: false),
                    NorthSouthPedestrian = table.Column<int>(type: "INTEGER", nullable: false),
                    EastWestPedestrian = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signal_States", x => x.Location_Serial);
                    table.ForeignKey(
                        name: "FK_Signal_States_Locations_Location_Serial",
                        column: x => x.Location_Serial,
                        principalTable: "Locations",
                        principalColumn: "Location_Serial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Traffic_Telemetries",
                columns: table => new
                {
                    Device_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Recorded_At = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Approach = table.Column<int>(type: "INTEGER", nullable: false),
                    Vehicle_Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Speed_Kph = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traffic_Telemetries", x => new { x.Device_Serial, x.Recorded_At, x.Approach });
                    table.ForeignKey(
                        name: "FK_Traffic_Telemetries_IntersectionDevices_Device_Serial",
                        column: x => x.Device_Serial,
                        principalTable: "IntersectionDevices",
                        principalColumn: "Device_Serial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Violation_logs",
                columns: table => new
                {
                    Device_Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Occured_At = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Violation_Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Plate_Number = table.Column<string>(type: "TEXT", nullable: true),
                    Speed_Kph = table.Column<decimal>(type: "TEXT", nullable: true),
                    Signal_At_Time = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Violation_logs", x => new { x.Device_Serial, x.Occured_At });
                    table.ForeignKey(
                        name: "FK_Violation_logs_IntersectionDevices_Device_Serial",
                        column: x => x.Device_Serial,
                        principalTable: "IntersectionDevices",
                        principalColumn: "Device_Serial",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntersectionDevices_Location_Serial",
                table: "IntersectionDevices",
                column: "Location_Serial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Signal_Rules");

            migrationBuilder.DropTable(
                name: "Signal_States");

            migrationBuilder.DropTable(
                name: "Traffic_Telemetries");

            migrationBuilder.DropTable(
                name: "Violation_logs");

            migrationBuilder.DropTable(
                name: "IntersectionDevices");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
