using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HazardToSociety.Server.Migrations
{
    public partial class Datapoints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationDataPoints");

            migrationBuilder.DropIndex(
                name: "IX_Locations_Name",
                table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Datapoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", nullable: true),
                    Station = table.Column<string>(type: "TEXT", nullable: true),
                    Attributes = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Datapoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Datapoints_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationOfInterests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessedTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOfInterests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOfInterests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UpdateHistories",
                keyColumn: "UpdateType",
                keyValue: 1,
                column: "DateUpdated",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_City_Country",
                table: "Locations",
                columns: new[] { "City", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_NoaaId",
                table: "Locations",
                column: "NoaaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Datapoints_LocationId",
                table: "Datapoints",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationOfInterests_LocationId",
                table: "LocationOfInterests",
                column: "LocationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Datapoints");

            migrationBuilder.DropTable(
                name: "LocationOfInterests");

            migrationBuilder.DropIndex(
                name: "IX_Locations_City_Country",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_NoaaId",
                table: "Locations");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "LocationDataPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Average = table.Column<float>(type: "REAL", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Max = table.Column<float>(type: "REAL", nullable: false),
                    Min = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationDataPoints_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "UpdateHistories",
                keyColumn: "UpdateType",
                keyValue: 1,
                column: "DateUpdated",
                value: new DateTime(2021, 11, 10, 12, 1, 47, 313, DateTimeKind.Local).AddTicks(273));

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationDataPoints_LocationId",
                table: "LocationDataPoints",
                column: "LocationId");
        }
    }
}
