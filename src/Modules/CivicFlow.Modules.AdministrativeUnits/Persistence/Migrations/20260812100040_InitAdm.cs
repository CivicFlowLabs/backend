using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CivicFlow.Modules.AdministrativeUnits.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitAdm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "adm");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "administrative_unit",
                schema: "adm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    name_normalized = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    level = table.Column<short>(type: "smallint", nullable: false),
                    unit_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    parent_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    legal_basis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    data_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_authoritative = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrative_unit", x => x.id);
                    table.CheckConstraint("ck_adm_level", "level IN (1, 3)");
                    table.CheckConstraint("ck_adm_period", "valid_to IS NULL OR valid_to > valid_from");
                });

            migrationBuilder.CreateTable(
                name: "dataset_version",
                schema: "adm",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    version = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_version", x => x.id);
                    table.CheckConstraint("ck_singleton", "id = 1");
                });

            migrationBuilder.CreateTable(
                name: "unit_mapping",
                schema: "adm",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    old_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    old_level = table.Column<short>(type: "smallint", nullable: false),
                    old_full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    old_name_norm = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    new_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    new_level = table.Column<short>(type: "smallint", nullable: true),
                    mapping_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(4,3)", precision: 4, scale: 3, nullable: false, defaultValue: 1.000m),
                    note = table.Column<string>(type: "text", nullable: true),
                    effective_date = table.Column<DateOnly>(type: "date", nullable: false),
                    legal_basis = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_unit_mapping", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "administrative_boundary",
                schema: "adm",
                columns: table => new
                {
                    unit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    boundary = table.Column<MultiPolygon>(type: "geometry(MultiPolygon,4326)", nullable: false),
                    boundary_lod1 = table.Column<MultiPolygon>(type: "geometry(MultiPolygon,4326)", nullable: true),
                    boundary_lod2 = table.Column<MultiPolygon>(type: "geometry(MultiPolygon,4326)", nullable: true),
                    centroid = table.Column<Point>(type: "geometry(Point,4326)", nullable: false),
                    headquarters = table.Column<Point>(type: "geometry(Point,4326)", nullable: true),
                    bbox = table.Column<Polygon>(type: "geometry(Polygon,4326)", nullable: false),
                    area_km2 = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: true),
                    data_source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_updated = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrative_boundary", x => x.unit_id);
                    table.ForeignKey(
                        name: "fk_administrative_boundary_administrative_units_unit_id",
                        column: x => x.unit_id,
                        principalSchema: "adm",
                        principalTable: "administrative_unit",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bnd_centroid",
                schema: "adm",
                table: "administrative_boundary",
                column: "centroid")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_bnd_geom",
                schema: "adm",
                table: "administrative_boundary",
                column: "boundary")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_bnd_geom_lod1",
                schema: "adm",
                table: "administrative_boundary",
                column: "boundary_lod1")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "ix_adm_code",
                schema: "adm",
                table: "administrative_unit",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_adm_level_valid",
                schema: "adm",
                table: "administrative_unit",
                column: "level",
                filter: "valid_to IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_adm_name_trgm",
                schema: "adm",
                table: "administrative_unit",
                column: "name_normalized")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_adm_parent",
                schema: "adm",
                table: "administrative_unit",
                column: "parent_code",
                filter: "valid_to IS NULL");

            migrationBuilder.CreateIndex(
                name: "uq_adm_code_valid_from",
                schema: "adm",
                table: "administrative_unit",
                columns: new[] { "code", "valid_from" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_map_name_trgm",
                schema: "adm",
                table: "unit_mapping",
                column: "old_name_norm")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_map_new",
                schema: "adm",
                table: "unit_mapping",
                column: "new_code");

            migrationBuilder.CreateIndex(
                name: "ix_map_old",
                schema: "adm",
                table: "unit_mapping",
                columns: new[] { "old_code", "old_level" });

            // Viết tay vì EF Core không mô hình hoá được ràng buộc EXCLUDE.
            //
            // Đây là thứ bảo vệ tính đúng đắn của toàn bộ bảng có phiên bản
            // thời gian: một mã đơn vị không bao giờ được có hai bản ghi hiệu
            // lực chồng lấn nhau. Thiếu nó, dữ liệu vẫn nạp thành công nhưng
            // truy vấn theo mốc thời gian sẽ trả về nhiều hơn một bản ghi cho
            // cùng một mã, và lỗi chỉ lộ ra rất lâu về sau.
            //
            // Khoảng nửa mở '[)' — ngày valid_to không thuộc khoảng hiệu lực —
            // nên một đơn vị đóng ngày 30/06 và đơn vị mở ngày 01/07 không bị
            // coi là chồng lấn. Toán tử && cần btree_gist cho cột code kiểu
            // varchar; extension đã được bật ở đầu migration này.
            migrationBuilder.Sql("""
                ALTER TABLE adm.administrative_unit
                  ADD CONSTRAINT ex_adm_no_overlap
                  EXCLUDE USING gist (
                      code WITH =,
                      daterange(valid_from, valid_to, '[)') WITH &&
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE adm.administrative_unit "
                + "DROP CONSTRAINT IF EXISTS ex_adm_no_overlap;");

            migrationBuilder.DropTable(
                name: "administrative_boundary",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "dataset_version",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "unit_mapping",
                schema: "adm");

            migrationBuilder.DropTable(
                name: "administrative_unit",
                schema: "adm");
        }
    }
}
