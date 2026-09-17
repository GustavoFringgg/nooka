using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nooka.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWordCategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Words");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Words",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
