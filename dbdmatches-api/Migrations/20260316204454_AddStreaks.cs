using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DbdMatches.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStreaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "streak_killers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    killer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    current_streak = table.Column<int>(type: "integer", nullable: false),
                    best_streak = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streak_killers", x => x.id);
                    table.ForeignKey(
                        name: "fk_streak_killers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "streaks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    current_overall = table.Column<int>(type: "integer", nullable: false),
                    best_overall = table.Column<int>(type: "integer", nullable: false),
                    current_killer = table.Column<int>(type: "integer", nullable: false),
                    best_killer = table.Column<int>(type: "integer", nullable: false),
                    current_survivor = table.Column<int>(type: "integer", nullable: false),
                    best_survivor = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streaks", x => x.id);
                    table.ForeignKey(
                        name: "fk_streaks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_streak_killers_user_id_killer",
                table: "streak_killers",
                columns: new[] { "user_id", "killer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_streaks_user_id",
                table: "streaks",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "streak_killers");

            migrationBuilder.DropTable(
                name: "streaks");
        }
    }
}
