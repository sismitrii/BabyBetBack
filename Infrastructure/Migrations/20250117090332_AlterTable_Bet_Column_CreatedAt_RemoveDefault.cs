using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_Bet_Column_CreatedAt_RemoveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Bet",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2025, 1, 17, 10, 0, 14, 797, DateTimeKind.Local).AddTicks(9750));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Bet",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 17, 10, 0, 14, 797, DateTimeKind.Local).AddTicks(9750),
                oldClrType: typeof(DateTime),
                oldType: "TEXT");
        }
    }
}
