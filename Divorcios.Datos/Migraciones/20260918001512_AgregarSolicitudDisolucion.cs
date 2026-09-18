using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarSolicitudDisolucion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "solicitud_disolucion",
                schema: "divorcios",
                columns: table => new
                {
                    solicitud_disolucion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_solicitud_id = table.Column<long>(type: "bigint", nullable: false),
                    fecha_presentacion_mesa_partes = table.Column<DateOnly>(type: "date", nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PRESENTADA"),
                    registrada_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    registrada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    validada_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    validada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitud_disolucion", x => x.solicitud_disolucion_id);
                    table.CheckConstraint("ck_solicitud_disolucion_estado", "estado_codigo IN (\r\n    'PRESENTADA',\r\n    'ADMITIDA',\r\n    'OBSERVADA',\r\n    'RECHAZADA'\r\n)");
                    table.CheckConstraint("ck_solicitud_disolucion_fechas", "validada_en IS NULL\r\nOR validada_en >= registrada_en");
                    table.CheckConstraint("ck_solicitud_disolucion_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_solicitud_disolucion_resultado_observacion", "estado_codigo NOT IN (\r\n    'OBSERVADA',\r\n    'RECHAZADA'\r\n)\r\nOR (\r\n    observacion IS NOT NULL\r\n    AND btrim(observacion) <> ''\r\n)");
                    table.CheckConstraint("ck_solicitud_disolucion_validacion", "(\r\n    estado_codigo = 'PRESENTADA'\r\n    AND validada_por_usuario_id IS NULL\r\n    AND validada_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'ADMITIDA',\r\n        'OBSERVADA',\r\n        'RECHAZADA'\r\n    )\r\n    AND validada_por_usuario_id IS NOT NULL\r\n    AND validada_en IS NOT NULL\r\n)");
                    table.ForeignKey(
                        name: "fk_solicitud_disolucion_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_disolucion_documento_documento_solicitud_id",
                        column: x => x.documento_solicitud_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_disolucion_usuarios_internos_registrada_por_usuar",
                        column: x => x.registrada_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitud_disolucion_usuarios_internos_validada_por_usuario",
                        column: x => x.validada_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitante_disolucion",
                schema: "divorcios",
                columns: table => new
                {
                    solicitante_disolucion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    solicitud_disolucion_id = table.Column<long>(type: "bigint", nullable: false),
                    caso_conyuge_id = table.Column<long>(type: "bigint", nullable: false),
                    modalidad_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "DIRECTA"),
                    representacion_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solicitante_disolucion", x => x.solicitante_disolucion_id);
                    table.CheckConstraint("ck_solicitante_disolucion_modalidad", "modalidad_codigo IN (\r\n    'DIRECTA',\r\n    'APODERADO'\r\n)");
                    table.CheckConstraint("ck_solicitante_disolucion_representacion", "(\r\n    modalidad_codigo = 'DIRECTA'\r\n    AND representacion_id IS NULL\r\n)\r\nOR\r\n(\r\n    modalidad_codigo = 'APODERADO'\r\n    AND representacion_id IS NOT NULL\r\n)");
                    table.ForeignKey(
                        name: "fk_solicitante_disolucion_caso_conyuge_caso_conyuge_id",
                        column: x => x.caso_conyuge_id,
                        principalSchema: "divorcios",
                        principalTable: "caso_conyuge",
                        principalColumn: "caso_conyuge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitante_disolucion_representacion_representacion_id",
                        column: x => x.representacion_id,
                        principalSchema: "divorcios",
                        principalTable: "representacion",
                        principalColumn: "representacion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_solicitante_disolucion_solicitudes_disolucion_solicitud_dis",
                        column: x => x.solicitud_disolucion_id,
                        principalSchema: "divorcios",
                        principalTable: "solicitud_disolucion",
                        principalColumn: "solicitud_disolucion_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_solicitante_disolucion_caso_conyuge_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                column: "caso_conyuge_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitante_disolucion_representacion_id",
                schema: "divorcios",
                table: "solicitante_disolucion",
                column: "representacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitante_disolucion_solicitud_disolucion_id_caso_conyuge",
                schema: "divorcios",
                table: "solicitante_disolucion",
                columns: new[] { "solicitud_disolucion_id", "caso_conyuge_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_caso_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "caso_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_documento_solicitud_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "documento_solicitud_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_estado_codigo",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "estado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_fecha_presentacion_mesa_partes",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "fecha_presentacion_mesa_partes");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_registrada_por_usuario_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "registrada_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_disolucion_validada_por_usuario_id",
                schema: "divorcios",
                table: "solicitud_disolucion",
                column: "validada_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "solicitante_disolucion",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "solicitud_disolucion",
                schema: "divorcios");
        }
    }
}
