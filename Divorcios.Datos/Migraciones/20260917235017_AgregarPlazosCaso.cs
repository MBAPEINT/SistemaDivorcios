using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarPlazosCaso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plazo_caso",
                schema: "divorcios",
                columns: table => new
                {
                    plazo_caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    regla_plazo_id = table.Column<short>(type: "smallint", nullable: false),
                    historial_estado_caso_origen_id = table.Column<long>(type: "bigint", nullable: true),
                    numero_aplicacion = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE"),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plazo_caso", x => x.plazo_caso_id);
                    table.CheckConstraint("ck_plazo_caso_aplicacion", "numero_aplicacion > 0");
                    table.CheckConstraint("ck_plazo_caso_cierre", "(\r\n    estado_codigo = 'PENDIENTE'\r\n    AND cerrado_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'CUMPLIDO',\r\n        'CANCELADO'\r\n    )\r\n    AND cerrado_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_plazo_caso_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'CUMPLIDO',\r\n    'CANCELADO'\r\n)");
                    table.CheckConstraint("ck_plazo_caso_fechas", "fecha_vencimiento >= fecha_inicio");
                    table.CheckConstraint("ck_plazo_caso_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.ForeignKey(
                        name: "fk_plazo_caso_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_historial_estado_caso_historial_estado_caso_orig",
                        column: x => x.historial_estado_caso_origen_id,
                        principalSchema: "divorcios",
                        principalTable: "historial_estado_caso",
                        principalColumn: "historial_estado_caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_reglas_plazo_regla_plazo_id",
                        column: x => x.regla_plazo_id,
                        principalSchema: "divorcios",
                        principalTable: "regla_plazo",
                        principalColumn: "regla_plazo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plazo_caso_usuarios_internos_creado_por_usuario_id",
                        column: x => x.creado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_caso_id_estado_codigo_fecha_vencimiento",
                schema: "divorcios",
                table: "plazo_caso",
                columns: new[] { "caso_id", "estado_codigo", "fecha_vencimiento" });

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_caso_id_regla_plazo_id_numero_aplicacion",
                schema: "divorcios",
                table: "plazo_caso",
                columns: new[] { "caso_id", "regla_plazo_id", "numero_aplicacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_creado_por_usuario_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "creado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_historial_estado_caso_origen_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "historial_estado_caso_origen_id");

            migrationBuilder.CreateIndex(
                name: "ix_plazo_caso_regla_plazo_id",
                schema: "divorcios",
                table: "plazo_caso",
                column: "regla_plazo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plazo_caso",
                schema: "divorcios");
        }
    }
}
