-- Sólo lectura: migraciones presentes y cantidades exactas por tabla, incluidos catálogos.
-- No consulta datos personales ni modifica estructura o registros.
BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ READ ONLY;

SELECT json_build_object(
    'base', current_database(),
    'version_postgresql', current_setting('server_version'),
    'solo_lectura', current_setting('transaction_read_only'),
    'historial_migraciones_presente', EXISTS (
        SELECT 1 FROM pg_tables
        WHERE tablename = '__EFMigrationsHistory'
          AND schemaname NOT IN ('pg_catalog', 'information_schema')),
    'tablas', COALESCE((
        SELECT json_agg(conteo ORDER BY esquema, tabla)
        FROM (
            SELECT schemaname AS esquema, tablename AS tabla,
                ((xpath('/row/total/text()', query_to_xml(
                    format('SELECT count(*) AS total FROM %I.%I', schemaname, tablename),
                    true, true, '')))[1]::text)::bigint AS registros
            FROM pg_tables
            WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
              AND schemaname NOT LIKE 'pg_toast%'
              AND schemaname NOT LIKE 'pg_temp%'
        ) conteo
    ), '[]'::json)
);

-- Se ejecuta únicamente si existe la tabla histórica. Soporta ambas convenciones de columnas.
SELECT format(
    'SELECT json_build_object(''migraciones'', COALESCE(json_agg(h ORDER BY COALESCE(to_jsonb(h)->>''migration_id'', to_jsonb(h)->>''MigrationId'')), ''[]''::json)) FROM %I.%I h',
    schemaname, tablename)
FROM pg_tables
WHERE tablename = '__EFMigrationsHistory'
  AND schemaname NOT IN ('pg_catalog', 'information_schema')
ORDER BY schemaname
\gexec

COMMIT;
