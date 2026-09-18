using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRevisionesPrerregistro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "revision_preregistro",
                schema: "divorcios",
                columns: table => new
                {
                    revision_preregistro_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    preregistro_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_revision = table.Column<short>(type: "smallint", nullable: false),
                    revisado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    resultado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "EN_REVISION"),
                    comentario_general = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    iniciada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    finalizada_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_revision_preregistro", x => x.revision_preregistro_id);
                    table.CheckConstraint("ck_revision_preregistro_comentario", "comentario_general IS NULL\r\nOR btrim(comentario_general) <> ''");
                    table.CheckConstraint("ck_revision_preregistro_fechas", "finalizada_en IS NULL\r\nOR finalizada_en >= iniciada_en");
                    table.CheckConstraint("ck_revision_preregistro_finalizacion", "(\r\n    resultado_codigo = 'EN_REVISION'\r\n    AND finalizada_en IS NULL\r\n)\r\nOR\r\n(\r\n    resultado_codigo IN (\r\n        'OBSERVADO',\r\n        'APROBADO'\r\n    )\r\n    AND finalizada_en IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_revision_preregistro_numero", "numero_revision > 0");
                    table.CheckConstraint("ck_revision_preregistro_resultado", "resultado_codigo IN (\r\n    'EN_REVISION',\r\n    'OBSERVADO',\r\n    'APROBADO'\r\n)");
                    table.ForeignKey(
                        name: "fk_revision_preregistro_preregistro_preregistro_id",
                        column: x => x.preregistro_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro",
                        principalColumn: "preregistro_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_preregistro_usuarios_internos_revisado_por_usuario",
                        column: x => x.revisado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "revision_detalle",
                schema: "divorcios",
                columns: table => new
                {
                    revision_detalle_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    revision_preregistro_id = table.Column<long>(type: "bigint", nullable: false),
                    preregistro_requisito_id = table.Column<long>(type: "bigint", nullable: false),
                    resultado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_revision_detalle", x => x.revision_detalle_id);
                    table.CheckConstraint("ck_revision_detalle_observacion_vacia", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_revision_detalle_observado", "resultado_codigo <> 'OBSERVADO'\r\nOR (\r\n    observacion IS NOT NULL\r\n    AND btrim(observacion) <> ''\r\n)");
                    table.CheckConstraint("ck_revision_detalle_resultado", "resultado_codigo IN (\r\n    'CONFORME',\r\n    'OBSERVADO',\r\n    'NO_APLICA'\r\n)");
                    table.ForeignKey(
                        name: "fk_revision_detalle_preregistro_requisito_preregistro_requisit",
                        column: x => x.preregistro_requisito_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro_requisito",
                        principalColumn: "preregistro_requisito_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_revision_detalle_revisiones_prerregistro_revision_preregist",
                        column: x => x.revision_preregistro_id,
                        principalSchema: "divorcios",
                        principalTable: "revision_preregistro",
                        principalColumn: "revision_preregistro_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_revision_detalle_preregistro_requisito_id",
                schema: "divorcios",
                table: "revision_detalle",
                column: "preregistro_requisito_id");

            migrationBuilder.CreateIndex(
                name: "ix_revision_detalle_resultado_codigo",
                schema: "divorcios",
                table: "revision_detalle",
                column: "resultado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_revision_detalle_revision_preregistro_id_preregistro_requis",
                schema: "divorcios",
                table: "revision_detalle",
                columns: new[] { "revision_preregistro_id", "preregistro_requisito_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revision_preregistro_preregistro_id_numero_revision",
                schema: "divorcios",
                table: "revision_preregistro",
                columns: new[] { "preregistro_id", "numero_revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revision_preregistro_resultado_codigo",
                schema: "divorcios",
                table: "revision_preregistro",
                column: "resultado_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_revision_preregistro_revisado_por_usuario_id",
                schema: "divorcios",
                table: "revision_preregistro",
                column: "revisado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "revision_detalle",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "revision_preregistro",
                schema: "divorcios");
        }
    }
}
