using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarCatalogosBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "destino_oficio",
                schema: "divorcios",
                columns: table => new
                {
                    destino_oficio_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_destino_oficio", x => x.destino_oficio_id);
                });

            migrationBuilder.CreateTable(
                name: "estado_caso",
                schema: "divorcios",
                columns: table => new
                {
                    estado_caso_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    nombre_ciudadano = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    etapa_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    orden_visual = table.Column<short>(type: "smallint", nullable: false),
                    es_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_estado_caso", x => x.estado_caso_id);
                    table.CheckConstraint("ck_estado_etapa", "etapa_codigo IN ('PRERREGISTRO', 'SEPARACION', 'ESPERA', 'DIVORCIO', 'CIERRE')");
                    table.CheckConstraint("ck_estado_orden", "orden_visual > 0");
                });

            migrationBuilder.CreateTable(
                name: "regla_plazo",
                schema: "divorcios",
                columns: table => new
                {
                    regla_plazo_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    cantidad = table.Column<short>(type: "smallint", nullable: false),
                    unidad_codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    tipo_dia_codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    vigente_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    vigente_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    fuente = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regla_plazo", x => x.regla_plazo_id);
                    table.CheckConstraint("ck_regla_cantidad", "cantidad > 0");
                    table.CheckConstraint("ck_regla_tipo_dia", "tipo_dia_codigo IN ('CALENDARIO', 'HABIL', 'OPERATIVO')");
                    table.CheckConstraint("ck_regla_unidad", "unidad_codigo IN ('DIA', 'MES')");
                    table.CheckConstraint("ck_regla_vigencia", "vigente_hasta IS NULL OR vigente_hasta >= vigente_desde");
                });

            migrationBuilder.CreateTable(
                name: "requisito_catalogo",
                schema: "divorcios",
                columns: table => new
                {
                    requisito_catalogo_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_requisito_catalogo", x => x.requisito_catalogo_id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_documento",
                schema: "divorcios",
                columns: table => new
                {
                    tipo_documento_id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nombre = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    origen_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipo_documento", x => x.tipo_documento_id);
                    table.CheckConstraint("ck_tipo_documento_origen", "origen_codigo IN ('CIUDADANO', 'MUNICIPALIDAD', 'AMBOS')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_destino_oficio_codigo",
                schema: "divorcios",
                table: "destino_oficio",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estado_caso_codigo",
                schema: "divorcios",
                table: "estado_caso",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_estado_caso_etapa_codigo_orden_visual",
                schema: "divorcios",
                table: "estado_caso",
                columns: new[] { "etapa_codigo", "orden_visual" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regla_plazo_codigo_vigente_desde",
                schema: "divorcios",
                table: "regla_plazo",
                columns: new[] { "codigo", "vigente_desde" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_requisito_catalogo_codigo",
                schema: "divorcios",
                table: "requisito_catalogo",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tipo_documento_codigo",
                schema: "divorcios",
                table: "tipo_documento",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "destino_oficio",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "estado_caso",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "regla_plazo",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "requisito_catalogo",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "tipo_documento",
                schema: "divorcios");
        }
    }
}
