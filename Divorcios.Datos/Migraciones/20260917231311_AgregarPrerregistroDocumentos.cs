using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Divorcios.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarPrerregistroDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "preregistro",
                schema: "divorcios",
                columns: table => new
                {
                    preregistro_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    fecha_matrimonio = table.Column<DateOnly>(type: "date", nullable: false),
                    matrimonio_en_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_domicilio_conyugal_en_porvenir = table.Column<bool>(type: "boolean", nullable: false),
                    mutuo_acuerdo_declarado = table.Column<bool>(type: "boolean", nullable: false),
                    cantidad_hijos_menores = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    cantidad_hijos_mayores_incapaces = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    tiene_bienes_sociales = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enviado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aprobado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preregistro", x => x.preregistro_id);
                    table.CheckConstraint("ck_preregistro_aprobacion", "aprobado_en IS NULL\r\nOR (\r\n    enviado_en IS NOT NULL\r\n    AND aprobado_en >= enviado_en\r\n)");
                    table.CheckConstraint("ck_preregistro_envio", "enviado_en IS NULL OR enviado_en >= creado_en");
                    table.CheckConstraint("ck_preregistro_hijos_mayores_incapaces", "cantidad_hijos_mayores_incapaces >= 0");
                    table.CheckConstraint("ck_preregistro_hijos_menores", "cantidad_hijos_menores >= 0");
                    table.ForeignKey(
                        name: "fk_preregistro_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "preregistro_requisito",
                schema: "divorcios",
                columns: table => new
                {
                    preregistro_requisito_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    preregistro_id = table.Column<long>(type: "bigint", nullable: false),
                    requisito_catalogo_id = table.Column<short>(type: "smallint", nullable: false),
                    obligatorio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    estado_codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDIENTE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_preregistro_requisito", x => x.preregistro_requisito_id);
                    table.CheckConstraint("ck_preregistro_requisito_estado", "estado_codigo IN (\r\n    'PENDIENTE',\r\n    'CARGADO',\r\n    'CONFORME',\r\n    'OBSERVADO',\r\n    'NO_APLICA'\r\n)");
                    table.ForeignKey(
                        name: "fk_preregistro_requisito_preregistro_preregistro_id",
                        column: x => x.preregistro_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro",
                        principalColumn: "preregistro_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_preregistro_requisito_requisitos_catalogo_requisito_catalog",
                        column: x => x.requisito_catalogo_id,
                        principalSchema: "divorcios",
                        principalTable: "requisito_catalogo",
                        principalColumn: "requisito_catalogo_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento",
                schema: "divorcios",
                columns: table => new
                {
                    documento_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    caso_id = table.Column<long>(type: "bigint", nullable: false),
                    tipo_documento_id = table.Column<short>(type: "smallint", nullable: false),
                    preregistro_requisito_id = table.Column<long>(type: "bigint", nullable: true),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    etapa_codigo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento", x => x.documento_id);
                    table.CheckConstraint("ck_documento_etapa", "etapa_codigo IN (\r\n    'PRERREGISTRO',\r\n    'SEPARACION',\r\n    'DIVORCIO',\r\n    'CIERRE'\r\n)");
                    table.CheckConstraint("ck_documento_titulo", "btrim(titulo) <> ''");
                    table.ForeignKey(
                        name: "fk_documento_caso_caso_id",
                        column: x => x.caso_id,
                        principalSchema: "divorcios",
                        principalTable: "caso",
                        principalColumn: "caso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_preregistros_requisitos_preregistro_requisito_id",
                        column: x => x.preregistro_requisito_id,
                        principalSchema: "divorcios",
                        principalTable: "preregistro_requisito",
                        principalColumn: "preregistro_requisito_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_tipos_documento_tipo_documento_id",
                        column: x => x.tipo_documento_id,
                        principalSchema: "divorcios",
                        principalTable: "tipo_documento",
                        principalColumn: "tipo_documento_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento_version",
                schema: "divorcios",
                columns: table => new
                {
                    documento_version_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    documento_id = table.Column<long>(type: "bigint", nullable: false),
                    numero_version = table.Column<short>(type: "smallint", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamano_bytes = table.Column<long>(type: "bigint", nullable: false),
                    almacenamiento_clave = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sha256 = table.Column<string>(type: "character(64)", nullable: false),
                    cargado_por_cuenta_id = table.Column<long>(type: "bigint", nullable: true),
                    cargado_por_usuario_id = table.Column<long>(type: "bigint", nullable: true),
                    cargado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documento_version", x => x.documento_version_id);
                    table.CheckConstraint("ck_documento_version_cargador", "(\r\n    cargado_por_cuenta_id IS NOT NULL\r\n    AND cargado_por_usuario_id IS NULL\r\n)\r\nOR\r\n(\r\n    cargado_por_cuenta_id IS NULL\r\n    AND cargado_por_usuario_id IS NOT NULL\r\n)");
                    table.CheckConstraint("ck_documento_version_clave", "btrim(almacenamiento_clave) <> ''");
                    table.CheckConstraint("ck_documento_version_mime", "btrim(mime_type) <> ''");
                    table.CheckConstraint("ck_documento_version_nombre", "btrim(nombre_archivo) <> ''");
                    table.CheckConstraint("ck_documento_version_numero", "numero_version > 0");
                    table.CheckConstraint("ck_documento_version_sha256", "sha256 ~ '^[0-9a-f]{64}$'");
                    table.CheckConstraint("ck_documento_version_tamano", "tamano_bytes > 0");
                    table.ForeignKey(
                        name: "fk_documento_version_cuenta_ciudadana_cargado_por_cuenta_id",
                        column: x => x.cargado_por_cuenta_id,
                        principalSchema: "divorcios",
                        principalTable: "cuenta_ciudadana",
                        principalColumn: "cuenta_ciudadana_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_version_documento_documento_id",
                        column: x => x.documento_id,
                        principalSchema: "divorcios",
                        principalTable: "documento",
                        principalColumn: "documento_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documento_version_usuarios_internos_cargado_por_usuario_id",
                        column: x => x.cargado_por_usuario_id,
                        principalSchema: "divorcios",
                        principalTable: "usuario_interno",
                        principalColumn: "usuario_interno_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_caso_id_etapa_codigo",
                schema: "divorcios",
                table: "documento",
                columns: new[] { "caso_id", "etapa_codigo" });

            migrationBuilder.CreateIndex(
                name: "ix_documento_preregistro_requisito_id",
                schema: "divorcios",
                table: "documento",
                column: "preregistro_requisito_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_tipo_documento_id",
                schema: "divorcios",
                table: "documento",
                column: "tipo_documento_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_version_almacenamiento_clave",
                schema: "divorcios",
                table: "documento_version",
                column: "almacenamiento_clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documento_version_cargado_por_cuenta_id",
                schema: "divorcios",
                table: "documento_version",
                column: "cargado_por_cuenta_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_version_cargado_por_usuario_id",
                schema: "divorcios",
                table: "documento_version",
                column: "cargado_por_usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_documento_version_documento_id_numero_version",
                schema: "divorcios",
                table: "documento_version",
                columns: new[] { "documento_id", "numero_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_documento_version_sha256",
                schema: "divorcios",
                table: "documento_version",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_caso_id",
                schema: "divorcios",
                table: "preregistro",
                column: "caso_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_requisito_preregistro_id_requisito_catalogo_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                columns: new[] { "preregistro_id", "requisito_catalogo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_preregistro_requisito_requisito_catalogo_id",
                schema: "divorcios",
                table: "preregistro_requisito",
                column: "requisito_catalogo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_version",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "documento",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "preregistro_requisito",
                schema: "divorcios");

            migrationBuilder.DropTable(
                name: "preregistro",
                schema: "divorcios");
        }
    }
}
