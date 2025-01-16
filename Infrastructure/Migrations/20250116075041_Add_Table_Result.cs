using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class Add_Table_Result : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Result",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BetGameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Gender = table.Column<int>(type: "INTEGER", nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    BirthTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    Size = table.Column<int>(type: "INTEGER", nullable: true),
                    Weight = table.Column<double>(type: "REAL", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Result", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Result_BetGame_BetGameId",
                        column: x => x.BetGameId,
                        principalTable: "BetGame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Result",
                columns: new[] { "Id", "BetGameId", "BirthDate", "BirthTime", "Gender", "Name", "Size", "Weight" },
                values: new object[] { new Guid("600c3dd3-2045-4ff7-b0ee-7db638a038ec"), new Guid("f570d572-7098-464c-8a2f-8cc3ed486c0a"), null, null, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Result_BetGameId",
                table: "Result",
                column: "BetGameId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Result");
        }
    }
}
