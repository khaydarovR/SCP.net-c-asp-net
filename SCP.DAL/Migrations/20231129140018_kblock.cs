using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCP.DAL.Migrations
{
    /// <inheritdoc />
    public partial class kblock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 29, 14, 0, 18, 94, DateTimeKind.Utc).AddTicks(5958),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 28, 7, 16, 8, 196, DateTimeKind.Utc).AddTicks(3682));

            _ = migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "ApiKeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "ApiKeys");

            _ = migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2023, 11, 28, 7, 16, 8, 196, DateTimeKind.Utc).AddTicks(3682),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValue: new DateTime(2023, 11, 29, 14, 0, 18, 94, DateTimeKind.Utc).AddTicks(5958));
        }
    }
}
