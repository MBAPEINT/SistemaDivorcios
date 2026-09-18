using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRegistroAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "registro_auditoria",
                schema: "divorcios",
                columns: table => new
                {
                    registro_auditoria_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    actor_tipo_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    usuario_interno_id = table.Column<long>(type: "bigint", nullable: true),
                    cuenta_ciudadana_id = table.Column<long>(type: "bigint", nullable: true),
                    caso_id = table.Column<long>(type: "bigint", nullable: true),
                    accion_codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    recurso_codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    recurso_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resultado_codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false, defaultValue: "EXITO"),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    detalle_json = table.Column<string>(type: "jsonb", nullable: true),
                    direccion_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    correlacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_registro_auditoria", x => x.registro_auditoria_id);
                    table.CheckConstraint("ck_auditoria_accion", "btrim(accion_codigo) <> ''");
                    table.CheckConstraint("ck_auditoria_actor", "(\r\n    actor_tipo_codigo IN (\r\n        'SISTEMA',\r\n        'PUBLICO'\r\n    )\r\n    AND usuario_interno_id IS NULL\r\n    AND cuenta_ciudadana_id IS NULL\r\n)\r\nOR\r\n(\r\n    actor_tipo_codigo = 'USUARIO_INTERNO'\r\n    AND usuario_interno_id IS NOT NULL\r\n    AND cuenta_ciudadana_id IS NULL\r\n)\r\nOR\r\n(\r\n    actor_tipo_codigo = 'CUENTA_CIUDADANA'\r\n    AND usuario_interno_id IS NULL\r\n    AND cuenta_ciudadana_id IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_auditoria_actor_tipo", "actor_tipo_codigo IN (\r\n    'SISTEMA',\r\n    'PUBLICO',\r\n    'USUARIO_INTERNO',\r\n    'CUENTA_CIUDADANA'\r\n)");
                    table.CheckConstraint("ck_auditoria_descripcion", "descripcion IS NULL\r\nOR btrim(descripcion) <> ''");
                    table.CheckConstraint("ck_auditoria_ip", "direccion_ip IS NULL\r\nOR btrim(direccion_ip) <> ''");
                    table.CheckConstraint("ck_auditoria_recurso", "btrim(recurso_codigo) <> ''");
                    table.CheckConstraint("ck_auditoria_recurso_id", "recurso_id IS NULL\r\nOR btrim(recurso_id) <> ''");
                    table.CheckConstraint("ck_auditoria_resultado", "resultado_codigo IN (\r\n    'EXITO',\r\n    'RECHAZADO',\r\n    'ERROR'\r\n)");
                    table.CheckConstraint("ck_auditoria_user_agent", "user_agent IS NULL\r\nOR btrim(user_agent) <> ''");
                    table.ForeignKey(
                        name: "fk_registro_auditoria_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_auditoria_cuenta_ciudadana_cuenta_ciudadana_id",
                        column: x => x.cuenta_ciudadana_id,
                        principalSchema: "divorcios",
                        principalTable: "cuenta_ciudadana",
                        principalColumn: "cuenta_ciudadana_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_registro_auditoria_usuarios_internos_usuario_interno_id",
                        column: x => x.usuario_interno_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_accion_codigo_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "accion_codigo", "registrado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_caso_id_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "caso_id", "registrado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_correlacion_id",
                schema: "divorcios",
                table: "registro_auditoria",
                column: "correlacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_cuenta_ciudadana_id_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "cuenta_ciudadana_id", "registrado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_recurso_codigo_recurso_id",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "recurso_codigo", "recurso_id" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_resultado_codigo_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "resultado_codigo", "registrado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_auditoria_usuario_interno_id_registrado_en",
                schema: "divorcios",
                table: "registro_auditoria",
                columns: new[] { "usuario_interno_id", "registrado_en" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registro_auditoria",
                schema: "divorcios");
        }
    }
}
