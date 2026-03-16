using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DbdMatches.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_killers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    killer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sacrifices = table.Column<int>(type: "integer", nullable: false),
                    kills = table.Column<int>(type: "integer", nullable: false),
                    power_stat1 = table.Column<int>(type: "integer", nullable: false),
                    power_stat2 = table.Column<int>(type: "integer", nullable: false),
                    bloodpoints_earned = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    played_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_killers", x => x.id);
                    table.ForeignKey(
                        name: "fk_match_killers_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "match_survivors",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    public_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    escaped = table.Column<bool>(type: "boolean", nullable: false),
                    hatch_escape = table.Column<bool>(type: "boolean", nullable: false),
                    generators_completed = table.Column<double>(type: "double precision", nullable: false),
                    bloodpoints_earned = table.Column<int>(type: "integer", nullable: false),
                    result = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    played_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_match_survivors", x => x.id);
                    table.ForeignKey(
                        name: "fk_match_survivors_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_stats_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    stat_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    stat_value = table.Column<double>(type: "double precision", nullable: false),
                    fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_stats_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_stats_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_match_killers_public_id",
                table: "match_killers",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_killers_user_id",
                table: "match_killers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_match_survivors_public_id",
                table: "match_survivors",
                column: "public_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_survivors_user_id",
                table: "match_survivors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_stats_snapshots_user_id_stat_name",
                table: "player_stats_snapshots",
                columns: new[] { "user_id", "stat_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_killers");

            migrationBuilder.DropTable(
                name: "match_survivors");

            migrationBuilder.DropTable(
                name: "player_stats_snapshots");
        }
    }
}
