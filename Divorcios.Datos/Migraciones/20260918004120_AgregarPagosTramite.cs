using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarPagosTramite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pago_tramite",
                schema: "divorcios",
                columns: table => new
                {
                    pago_tramite_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    concepto_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    numero_voucher = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    moneda_codigo = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    fecha_pago = table.Column<DateOnly>(type: "date", nullable: false),
                    documento_comprobante_id = table.Column<long>(type: "bigint", nullable: true),
                    registrado_por_usuario_id = table.Column<long>(type: "bigint", nullable: false),
                    registrado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    observacion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pago_tramite", x => x.pago_tramite_id);
                    table.CheckConstraint("ck_pago_tramite_concepto", "concepto_codigo IN (\r\n    'COPIAS_CERTIFICADAS',\r\n    'TASA_PROCEDIMIENTO',\r\n    'OTRO'\r\n)");
                    table.CheckConstraint("ck_pago_tramite_moneda", "moneda_codigo = 'PEN'");
                    table.CheckConstraint("ck_pago_tramite_monto", "monto > 0");
                    table.CheckConstraint("ck_pago_tramite_observacion", "observacion IS NULL\r\nOR btrim(observacion) <> ''");
                    table.CheckConstraint("ck_pago_tramite_otro", "concepto_codigo <> 'OTRO'\r\nOR (\r\n    observacion IS NOT NULL\r\n    AND btrim(observacion) <> ''\r\n)");
                    table.CheckConstraint("ck_pago_tramite_voucher", "btrim(numero_voucher) <> ''");
                    table.ForeignKey(
                        name: "fk_pago_tramite_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_tramite_documento_documento_comprobante_id",
                        column: x => x.documento_comprobante_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pago_tramite_usuarios_internos_registrado_por_usuario_id",
                        column: x => x.registrado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_caso_id_fecha_pago",
                schema: "divorcios",
                table: "pago_tramite",
                columns: new[] { "caso_id", "fecha_pago" });

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_concepto_codigo",
                schema: "divorcios",
                table: "pago_tramite",
                column: "concepto_codigo");

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_documento_comprobante_id",
                schema: "divorcios",
                table: "pago_tramite",
                column: "documento_comprobante_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_numero_voucher_fecha_pago",
                schema: "divorcios",
                table: "pago_tramite",
                columns: new[] { "numero_voucher", "fecha_pago" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pago_tramite_registrado_por_usuario_id",
                schema: "divorcios",
                table: "pago_tramite",
                column: "registrado_por_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pago_tramite",
                schema: "divorcios");
        }
    }
}
