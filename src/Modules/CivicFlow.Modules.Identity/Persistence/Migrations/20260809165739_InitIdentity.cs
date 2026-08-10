using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace CivicFlow.Modules.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "administrative_unit",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    boundary = table.Column<MultiPolygon>(type: "geometry(MultiPolygon, 4326)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_unit", x => x.id);
                    table.CheckConstraint("ck_admin_unit_type", "type IN ('commune', 'ward', 'special_zone')");
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    administrative_unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                    table.CheckConstraint("ck_app_user_contact", "email IS NOT NULL OR phone_number IS NOT NULL");
                    table.CheckConstraint("ck_app_user_role", "role IN ('citizen', 'officer', 'leader', 'admin')");
                    table.ForeignKey(
                        name: "fk_app_user_admin_unit",
                        column: x => x.administrative_unit_id,
                        principalSchema: "identity",
                        principalTable: "administrative_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_user",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_unit_boundary",
                schema: "identity",
                table: "administrative_unit",
                column: "boundary")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "ix_admin_unit_province",
                schema: "identity",
                table: "administrative_unit",
                column: "province_code");

            migrationBuilder.CreateIndex(
                name: "uq_admin_unit_code",
                schema: "identity",
                table: "administrative_unit",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_user_unit_role",
                schema: "identity",
                table: "app_user",
                columns: new[] { "administrative_unit_id", "role" },
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "uq_app_user_email",
                schema: "identity",
                table: "app_user",
                column: "email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "uq_app_user_phone",
                schema: "identity",
                table: "app_user",
                column: "phone_number",
                unique: true,
                filter: "phone_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_expires",
                schema: "identity",
                table: "refresh_token",
                column: "expires_at",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user",
                schema: "identity",
                table: "refresh_token",
                column: "user_id",
                filter: "revoked_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "uq_refresh_token",
                schema: "identity",
                table: "refresh_token",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "app_user",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "administrative_unit",
                schema: "identity");
        }
    }
}
