using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRepresentacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "representacion",
                schema: "divorcios",
                columns: table => new
                {
                    representacion_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_conyuge_id = table.Column<long>(type: "bigint", nullable: false),
                    representante_persona_id = table.Column<long>(type: "bigint", nullable: false),
                    documento_poder_id = table.Column<long>(type: "bigint", nullable: false),
                    vigente_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    vigente_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_representacion", x => x.representacion_id);
                    table.CheckConstraint("ck_representacion_vigencia", "vigente_hasta IS NULL\r\nOR vigente_hasta >= vigente_desde");
                    table.ForeignKey(
                        name: "fk_representacion_caso_conyuge_caso_conyuge_id",
                        column: x => x.caso_conyuge_id,
                        principalSchema: "divorcios",
                        principalTable: "caso_conyuge",
                        principalColumn: "caso_conyuge_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_representacion_documento_documento_poder_id",
                        column: x => x.documento_poder_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_representacion_persona_representante_persona_id",
                        column: x => x.representante_persona_id,
                        principalSchema: "divorcios",
                        principalTable: "persona",
                        principalColumn: "persona_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_representacion_caso_conyuge_id",
                schema: "divorcios",
                table: "representacion",
                column: "caso_conyuge_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_representacion_documento_poder_id",
                schema: "divorcios",
                table: "representacion",
                column: "documento_poder_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_representacion_representante_persona_id",
                schema: "divorcios",
                table: "representacion",
                column: "representante_persona_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "representacion",
                schema: "divorcios");
        }
    }
}
