using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace CivicFlow.Modules.Identity.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdministrativeUnitFromIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_app_user_admin_unit",
                schema: "identity",
                table: "app_user");

            migrationBuilder.DropTable(
                name: "administrative_unit",
                schema: "identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administrative_unit",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    boundary = table.Column<MultiPolygon>(type: "geometry(MultiPolygon, 4326)", nullable: true),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    province_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    province_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrative_unit", x => x.id);
                    table.CheckConstraint("ck_admin_unit_type", "type IN ('commune', 'ward', 'special_zone')");
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

            migrationBuilder.AddForeignKey(
                name: "fk_app_user_admin_unit",
                schema: "identity",
                table: "app_user",
                column: "administrative_unit_id",
                principalSchema: "identity",
                principalTable: "administrative_unit",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
