using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class EKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EKey",
                table: "Safes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 10, 8, 13, 39, 43, 307, DateTimeKind.Utc).AddTicks(8802),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 10, 8, 13, 28, 44, 959, DateTimeKind.Utc).AddTicks(2383));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EKey",
                table: "Safes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 10, 8, 13, 28, 44, 959, DateTimeKind.Utc).AddTicks(2383),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 10, 8, 13, 39, 43, 307, DateTimeKind.Utc).AddTicks(8802));
        }
    }
}
