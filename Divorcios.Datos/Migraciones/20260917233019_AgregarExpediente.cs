using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarExpediente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expediente",
                schema: "divorcios",
                columns: table => new
                {
                    expediente_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_expediente = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fecha_ingreso_mesa_partes = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    observacion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expediente", x => x.expediente_id);
                    table.CheckConstraint("ck_expediente_numero", "btrim(numero_expediente) <> ''");
                    table.CheckConstraint("ck_expediente_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.ForeignKey(
                        name: "fk_expediente_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expediente_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_expediente_caso_id",
                schema: "divorcios",
                table: "expediente",
                column: "caso_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_fecha_ingreso_mesa_partes",
                schema: "divorcios",
                table: "expediente",
                column: "fecha_ingreso_mesa_partes");

            migrationBuilder.CreateIndex(
                name: "ix_expediente_numero_expediente",
                schema: "divorcios",
                table: "expediente",
                column: "numero_expediente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expediente_registrado_por_usuario_id",
                schema: "divorcios",
                table: "expediente",
                column: "registrado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expediente",
                schema: "divorcios");
        }
    }
}
