using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AlterTable_BetGame_AddColumn_DueTermDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueTermDate",
                table: "BetGame",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "BetGame",
                keyColumn: "Id",
                keyValue: new Guid("f570d572-7098-464c-8a2f-8cc3ed486c0a"),
                column: "DueTermDate",
                value: new DateTime(2025, 5, 26, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueTermDate",
                table: "BetGame");
        }
    }
}
