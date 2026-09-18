using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarAudienciasRatificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audiencia_ratificacion",
                schema: "divorcios",
                columns: table => new
                {
                    audiencia_ratificacion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_programacion = table.Column<short>(type: "smallint", nullable: false),
                    fecha_hora_programada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PROGRAMADA"),
                    fecha_hora_realizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cerrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creada_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    creada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audiencia_ratificacion", x => x.audiencia_ratificacion_id);
                    table.CheckConstraint("ck_audiencia_estado", "estado_codigo IN (\r\n    'PROGRAMADA',\r\n    'REALIZADA',\r\n    'NO_REALIZADA',\r\n    'REPROGRAMADA',\r\n    'CANCELADA'\r\n)");
                    table.CheckConstraint("ck_audiencia_fechas_estado", "(\r\n    estado_codigo = 'PROGRAMADA'\r\n    AND fecha_hora_realizacion IS NULL\r\n    AND cerrado_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo = 'REALIZADA'\r\n    AND fecha_hora_realizacion IS NOT NULL\r\n    AND cerrado_en IS NOT NULL\r\n    AND cerrado_en >= fecha_hora_realizacion\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'NO_REALIZADA',\r\n        'REPROGRAMADA',\r\n        'CANCELADA'\r\n    )\r\n    AND fecha_hora_realizacion IS NULL\r\n    AND cerrado_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_audiencia_numero_programacion", "numero_programacion > 0");
                    table.CheckConstraint("ck_audiencia_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_audiencia_resultado_observacion", "estado_codigo IN (\r\n    'PROGRAMADA',\r\n    'REALIZADA'\r\n)\r\nOR (\r\n    observacion IS NOT NULL\r\n    AND btrim(observacion) <> ''\r\n)");
                    table.ForeignKey(
                        name: "fk_audiencia_ratificacion_casos_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_audiencia_ratificacion_usuarios_internos_creada_por_usuario",
                        column: x => x.creada_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asistencia_audiencia",
                schema: "divorcios",
                columns: table => new
                {
                    asistencia_audiencia_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    audiencia_ratificacion_id = table.Column<long>(type: "bigint", nullable: false),
                    caso_conyuge_id = table.Column<long>(type: "bigint", nullable: false),
                    modalidad_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "DIRECTA"),
                    representacion_id = table.Column<long>(type: "bigint", nullable: true),
                    asistio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ratifico_voluntad = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    identidad_verificada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asistencia_audiencia", x => x.asistencia_audiencia_id);
                    table.CheckConstraint("ck_asistencia_inasistencia", "asistio = TRUE\r\nOR (\r\n    ratifico_voluntad = FALSE\r\n    AND identidad_verificada_en IS NULL\r\n)");
                    table.CheckConstraint("ck_asistencia_modalidad", "modalidad_codigo IN (\r\n    'DIRECTA',\r\n    'APODERADO'\r\n)");
                    table.CheckConstraint("ck_asistencia_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_asistencia_ratificacion", "ratifico_voluntad = FALSE\r\nOR (\r\n    asistio = TRUE\r\n    AND identidad_verificada_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_asistencia_representacion", "(\r\n    modalidad_codigo = 'DIRECTA'\r\n    AND representacion_id IS NULL\r\n)\r\nOR\r\n(\r\n    modalidad_codigo = 'APODERADO'\r\n    AND representacion_id IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_asistencia_verificacion", "identidad_verificada_en IS NULL\r\nOR asistio = TRUE");
                    table.ForeignKey(
                        name: "fk_asistencia_audiencia_audiencias_ratificacion_audiencia_rati",
                        column: x => x.audiencia_ratificacion_id,
                        principalSchema: "divorcios",
                        principalTable: "audiencia_ratificacion",
                        principalColumn: "audiencia_ratificacion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asistencia_audiencia_casos_conyuges_caso_conyuge_id",
                        column: x => x.caso_conyuge_id,
                        principalSchema: "divorcios",
                        principalTable: "caso_conyuge",
                        principalColumn: "caso_conyuge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_asistencia_audiencia_representaciones_representacion_id",
                        column: x => x.representacion_id,
                        principalSchema: "divorcios",
                        principalTable: "representacion",
                        principalColumn: "representacion_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_audiencia_audiencia_ratificacion_id_caso_conyuge",
                schema: "divorcios",
                table: "asistencia_audiencia",
                columns: new[] { "audiencia_ratificacion_id", "caso_conyuge_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_audiencia_caso_conyuge_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                column: "caso_conyuge_id");

            migrationBuilder.CreateIndex(
                name: "ix_asistencia_audiencia_representacion_id",
                schema: "divorcios",
                table: "asistencia_audiencia",
                column: "representacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_audiencia_ratificacion_caso_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                column: "caso_id",
                unique: true,
                filter: "estado_codigo = 'PROGRAMADA'");

            migrationBuilder.CreateIndex(
                name: "ix_audiencia_ratificacion_caso_id_fecha_hora_programada",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                columns: new[] { "caso_id", "fecha_hora_programada" });

            migrationBuilder.CreateIndex(
                name: "ix_audiencia_ratificacion_caso_id_numero_programacion",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                columns: new[] { "caso_id", "numero_programacion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audiencia_ratificacion_creada_por_usuario_id",
                schema: "divorcios",
                table: "audiencia_ratificacion",
                column: "creada_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencia_audiencia",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "audiencia_ratificacion",
                schema: "divorcios");
        }
    }
}
