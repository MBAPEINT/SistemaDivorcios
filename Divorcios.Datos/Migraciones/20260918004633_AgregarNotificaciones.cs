using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarNotificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificacion",
                schema: "divorcios",
                columns: table => new
                {
                    notificacion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    persona_destinataria_id = table.Column<long>(type: "bigint", nullable: false),
                    canal_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "WHATSAPP"),
                    tipo_codigo = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    destino = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    plantilla_codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    contenido = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    estado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "PENDIENTE"),
                    proximo_intento_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finalizada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creada_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    creada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notificacion", x => x.notificacion_id);
                    table.CheckConstraint("ck_notificacion_canal", "canal_codigo IN (\r\n    'WHATSAPP',\r\n    'LLAMADA',\r\n    'CORREO'\r\n)");
                    table.CheckConstraint("ck_notificacion_contenido", "btrim(contenido) <> ''");
                    table.CheckConstraint("ck_notificacion_destino", "btrim(destino) <> ''");
                    table.CheckConstraint("ck_notificacion_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'PROCESANDO',\r\n    'ENVIADA',\r\n    'FALLIDA',\r\n    'CANCELADA'\r\n)");
                    table.CheckConstraint("ck_notificacion_finalizacion", "(\r\n    estado_codigo IN (\r\n        'PENDIENTE',\r\n        'PROCESANDO'\r\n    )\r\n    AND finalizada_en IS NULL\r\n)\r\nOR\r\n(\r\n    estado_codigo IN (\r\n        'ENVIADA',\r\n        'FALLIDA',\r\n        'CANCELADA'\r\n    )\r\n    AND finalizada_en IS NOT NULL\r\n    AND proximo_intento_en IS NULL\r\n)");
                    table.CheckConstraint("ck_notificacion_plantilla", "plantilla_codigo IS NULL\r\nOR btrim(plantilla_codigo) <> ''");
                    table.CheckConstraint("ck_notificacion_tipo", "tipo_codigo IN (\r\n    'PRERREGISTRO_OBSERVADO',\r\n    'PRERREGISTRO_APROBADO',\r\n    'AUDIENCIA_PROGRAMADA',\r\n    'AUDIENCIA_REPROGRAMADA',\r\n    'SOLICITUD_OBSERVADA',\r\n    'RESOLUCION_DISPONIBLE',\r\n    'TRAMITE_FINALIZADO'\r\n)");
                    table.ForeignKey(
                        name: "fk_notificacion_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacion_personas_persona_destinataria_id",
                        column: x => x.persona_destinataria_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notificacion_usuarios_internos_creada_por_usuario_id",
                        column: x => x.creada_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "intento_notificacion",
                schema: "divorcios",
                columns: table => new
                {
                    intento_notificacion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    notificacion_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_intento = table.Column<short>(type: "smallint", nullable: false),
                    resultado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "EN_PROCESO"),
                    codigo_http = table.Column<short>(type: "smallint", nullable: true),
                    proveedor_mensaje_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ejecutado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    iniciado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    finalizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intento_notificacion", x => x.intento_notificacion_id);
                    table.CheckConstraint("ck_intento_notificacion_error_vacio", "error IS NULL\r\nOR btrim(error) <> ''");
                    table.CheckConstraint("ck_intento_notificacion_fechas", "finalizado_en IS NULL\r\nOR finalizado_en >= iniciado_en");
                    table.CheckConstraint("ck_intento_notificacion_finalizacion", "(\r\n    resultado_codigo = 'EN_PROCESO'\r\n    AND finalizado_en IS NULL\r\n    AND codigo_http IS NULL\r\n    AND proveedor_mensaje_id IS NULL\r\n    AND error IS NULL\r\n)\r\nOR\r\n(\r\n    resultado_codigo = 'EXITOSO'\r\n    AND finalizado_en IS NOT NULL\r\n    AND error IS NULL\r\n)\r\nOR\r\n(\r\n    resultado_codigo = 'ERROR'\r\n    AND finalizado_en IS NOT NULL\r\n    AND error IS NOT NULL\r\n    AND btrim(error) <> ''\r\n)");
                    table.CheckConstraint("ck_intento_notificacion_http", "codigo_http IS NULL\r\nOR codigo_http BETWEEN 100 AND 599");
                    table.CheckConstraint("ck_intento_notificacion_numero", "numero_intento > 0");
                    table.CheckConstraint("ck_intento_notificacion_proveedor", "proveedor_mensaje_id IS NULL\r\nOR btrim(proveedor_mensaje_id) <> ''");
                    table.CheckConstraint("ck_intento_notificacion_resultado", "resultado_codigo IN (\r\n    'EN_PROCESO',\r\n    'EXITOSO',\r\n    'ERROR'\r\n)");
                    table.ForeignKey(
                        name: "fk_intento_notificacion_notificaciones_notificacion_id",
                        column: x => x.notificacion_id,
                        principalSchema: "divorcios",
                        principalTable: "notificacion",
                        principalColumn: "notificacion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_intento_notificacion_usuarios_internos_ejecutado_por_usuari",
                        column: x => x.ejecutado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_intento_notificacion_ejecutado_por_usuario_id",
                schema: "divorcios",
                table: "intento_notificacion",
                column: "ejecutado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_intento_notificacion_notificacion_id_numero_intento",
                schema: "divorcios",
                table: "intento_notificacion",
                columns: new[] { "notificacion_id", "numero_intento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_intento_notificacion_resultado_codigo",
                schema: "divorcios",
                table: "intento_notificacion",
                column: "resultado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_caso_id_creada_en",
                schema: "divorcios",
                table: "notificacion",
                columns: new[] { "caso_id", "creada_en" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_creada_por_usuario_id",
                schema: "divorcios",
                table: "notificacion",
                column: "creada_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_estado_codigo_proximo_intento_en",
                schema: "divorcios",
                table: "notificacion",
                columns: new[] { "estado_codigo", "proximo_intento_en" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_persona_destinataria_id",
                schema: "divorcios",
                table: "notificacion",
                column: "persona_destinataria_id");

            migrationBuilder.CreateIndex(
                name: "ix_notificacion_tipo_codigo",
                schema: "divorcios",
                table: "notificacion",
                column: "tipo_codigo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intento_notificacion",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "notificacion",
                schema: "divorcios");
        }
    }
}
