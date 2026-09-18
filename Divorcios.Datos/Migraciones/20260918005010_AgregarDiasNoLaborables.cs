using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarDiasNoLaborables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dia_no_laborable",
                schema: "divorcios",
                columns: table => new
                {
                    dia_no_laborable_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    nombre = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    tipo_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    ambito_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    excluye_dia_habil = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    excluye_dia_operativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dia_no_laborable", x => x.dia_no_laborable_id);
                    table.CheckConstraint("ck_dia_no_laborable_ambito", "ambito_codigo IN (\r\n    'NACIONAL',\r\n    'REGIONAL',\r\n    'MUNICIPAL'\r\n)");
                    table.CheckConstraint("ck_dia_no_laborable_aplicacion", "excluye_dia_habil = TRUE\r\nOR excluye_dia_operativo = TRUE");
                    table.CheckConstraint("ck_dia_no_laborable_nombre", "btrim(nombre) <> ''");
                    table.CheckConstraint("ck_dia_no_laborable_tipo", "tipo_codigo IN (\r\n    'FERIADO',\r\n    'NO_LABORABLE',\r\n    'CIERRE_INSTITUCIONAL'\r\n)");
                    table.ForeignKey(
                        name: "fk_dia_no_laborable_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dia_no_laborable_activo_fecha",
                schema: "divorcios",
                table: "dia_no_laborable",
                columns: new[] { "activo", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_dia_no_laborable_fecha",
                schema: "divorcios",
                table: "dia_no_laborable",
                column: "fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dia_no_laborable_registrado_por_usuario_id",
                schema: "divorcios",
                table: "dia_no_laborable",
                column: "registrado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dia_no_laborable",
                schema: "divorcios");
        }
    }
}
