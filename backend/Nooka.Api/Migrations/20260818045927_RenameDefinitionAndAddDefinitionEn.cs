using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nooka.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefinitionAndAddDefinitionEn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Definition",
                table: "Words",
                newName: "DefinitionEN");

            migrationBuilder.AddColumn<string>(
                name: "DefinitionCN",
                table: "Words",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefinitionCN",
                table: "Words");

            migrationBuilder.RenameColumn(
                name: "DefinitionEN",
                table: "Words",
                newName: "Definition");
        }
    }
}
