using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class initn4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SafeRights",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 14, 20, 49, 33, 764, DateTimeKind.Utc).AddTicks(210),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 14, 20, 27, 53, 18, DateTimeKind.Utc).AddTicks(4285));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SafeRights_AppUserId",
                table: "SafeRights",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeRights_Id",
                table: "SafeRights",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights");

            migrationBuilder.DropIndex(
                name: "IX_SafeRights_AppUserId",
                table: "SafeRights");

            migrationBuilder.DropIndex(
                name: "IX_SafeRights_Id",
                table: "SafeRights");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SafeRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 14, 20, 27, 53, 18, DateTimeKind.Utc).AddTicks(4285),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 14, 20, 49, 33, 764, DateTimeKind.Utc).AddTicks(210));

            migrationBuilder.AddPrimaryKey(
                name: "PK_SafeRights",
                table: "SafeRights",
                columns: new[] { "AppUserId", "SafeId" });
        }
    }
}
