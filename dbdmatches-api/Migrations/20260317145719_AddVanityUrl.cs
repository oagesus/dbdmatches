using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbdMatches.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVanityUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "vanity_url",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vanity_url",
                table: "users");
        }
    }
}
