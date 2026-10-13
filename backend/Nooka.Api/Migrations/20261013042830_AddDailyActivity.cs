using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nooka.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyActivities",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActivityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    CompletedRounds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyActivities", x => new { x.UserId, x.ActivityDate, x.Mode });
                    table.ForeignKey(
                        name: "FK_DailyActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyActivities");
        }
    }
}
