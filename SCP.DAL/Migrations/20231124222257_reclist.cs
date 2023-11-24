using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class reclist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecordRights_RecordId",
                table: "RecordRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 24, 22, 22, 57, 56, DateTimeKind.Utc).AddTicks(1018),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 24, 22, 1, 10, 565, DateTimeKind.Utc).AddTicks(4184));

            migrationBuilder.CreateIndex(
                name: "IX_RecordRights_RecordId",
                table: "RecordRights",
                column: "RecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecordRights_RecordId",
                table: "RecordRights");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 24, 22, 1, 10, 565, DateTimeKind.Utc).AddTicks(4184),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 24, 22, 22, 57, 56, DateTimeKind.Utc).AddTicks(1018));

            migrationBuilder.CreateIndex(
                name: "IX_RecordRights_RecordId",
                table: "RecordRights",
                column: "RecordId",
                unique: true);
        }
    }
}
