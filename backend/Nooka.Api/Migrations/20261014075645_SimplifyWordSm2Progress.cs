using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nooka.Api.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyWordSm2Progress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interval",
                table: "WordSm2Progresses");

            migrationBuilder.DropColumn(
                name: "NextReviewDate",
                table: "WordSm2Progresses");

            migrationBuilder.DropColumn(
                name: "Repetitions",
                table: "WordSm2Progresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "WordSm2Progresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NextReviewDate",
                table: "WordSm2Progresses",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Repetitions",
                table: "WordSm2Progresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
