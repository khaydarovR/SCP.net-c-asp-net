using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClaimDead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeadDate",
                table: "SafeRights",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 8, 17, 41, 45, 124, DateTimeKind.Utc).AddTicks(7057),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 10, 13, 19, 12, 19, 107, DateTimeKind.Utc).AddTicks(9166));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadDate",
                table: "SafeRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 10, 13, 19, 12, 19, 107, DateTimeKind.Utc).AddTicks(9166),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 8, 17, 41, 45, 124, DateTimeKind.Utc).AddTicks(7057));
        }
    }
}
