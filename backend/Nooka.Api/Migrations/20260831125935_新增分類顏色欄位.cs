using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nooka.Api.Migrations
{
    /// <inheritdoc />
    public partial class 新增分類顏色欄位 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Categories");
        }
    }
}
