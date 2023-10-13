using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class _3pk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BotRights",
                table: "BotRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 10, 13, 19, 12, 19, 107, DateTimeKind.Utc).AddTicks(9166),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 10, 13, 17, 30, 58, 755, DateTimeKind.Utc).AddTicks(2789));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights",
                columns: new[] { "SafeId", "AppUserId", "ClaimValue" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BotRights",
                table: "BotRights",
                columns: new[] { "SafeId", "BotId", "ClaimValue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BotRights",
                table: "BotRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "At",
                table: "ActivityLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 10, 13, 17, 30, 58, 755, DateTimeKind.Utc).AddTicks(2789),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 10, 13, 19, 12, 19, 107, DateTimeKind.Utc).AddTicks(9166));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights",
                columns: new[] { "SafeId", "AppUserId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_BotRights",
                table: "BotRights",
                columns: new[] { "SafeId", "BotId" });
        }
    }
}
