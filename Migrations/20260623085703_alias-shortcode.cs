using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace URL_Shortener.Migrations
{
    /// <inheritdoc />
    public partial class aliasshortcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomAlias",
                table: "ShortenedURLs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomAlias",
                table: "ShortenedURLs",
                type: "TEXT",
                nullable: true);
        }
    }
}
