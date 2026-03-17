using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbdMatches.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsContaminated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_contaminated",
                table: "match_survivors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_contaminated",
                table: "match_killers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_contaminated",
                table: "match_survivors");

            migrationBuilder.DropColumn(
                name: "is_contaminated",
                table: "match_killers");
        }
    }
}
