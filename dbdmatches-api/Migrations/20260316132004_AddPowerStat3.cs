using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DbdMatches.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPowerStat3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "power_stat3",
                table: "match_killers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "power_stat3",
                table: "match_killers");
        }
    }
}
