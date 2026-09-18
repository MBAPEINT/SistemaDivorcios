using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarHistorialEstadosCaso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historial_estado_caso",
                schema: "divorcios",
                columns: table => new
                {
                    historial_estado_caso_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    estado_caso_id = table.Column<short>(type: "smallint", nullable: false),
                    numero_secuencia = table.Column<int>(type: "integer", nullable: false),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    iniciado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finalizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_historial_estado_caso", x => x.historial_estado_caso_id);
                    table.CheckConstraint("ck_historial_estado_fechas", "finalizado_en IS NULL\r\nOR finalizado_en >= iniciado_en");
                    table.CheckConstraint("ck_historial_estado_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_historial_estado_secuencia", "numero_secuencia > 0");
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_estado_caso_estado_caso_id",
                        column: x => x.estado_caso_id,
                        principalSchema: "divorcios",
                        principalTable: "estado_caso",
                        principalColumn: "estado_caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_historial_estado_caso_usuarios_internos_registrado_por_usua",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "caso_id",
                unique: true,
                filter: "finalizado_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id_iniciado_en",
                schema: "divorcios",
                table: "historial_estado_caso",
                columns: new[] { "caso_id", "iniciado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_caso_id_numero_secuencia",
                schema: "divorcios",
                table: "historial_estado_caso",
                columns: new[] { "caso_id", "numero_secuencia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_estado_caso_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "estado_caso_id");

            migrationBuilder.CreateIndex(
                name: "ix_historial_estado_caso_registrado_por_usuario_id",
                schema: "divorcios",
                table: "historial_estado_caso",
                column: "registrado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_estado_caso",
                schema: "divorcios");
        }
    }
}
