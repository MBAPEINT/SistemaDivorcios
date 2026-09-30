# Aplicación autorizada en la nueva base local

Fecha: 30 de septiembre de 2026. El usuario autorizó aplicar la cadena completa únicamente a la nueva base de desarrollo auditada. La operación terminó correctamente. La base de la otra PC no se consultó ni modificó.

## Verificación anterior a la aplicación

- Contenedor existente: `divorcios_postgres`, ID `72edbe8381eda9ca6f6e340d0ce00b236a9636cd59fbbeb04207e817406b746c`, saludable.
- Volumen conservado: `sistemadivorcios_divorcios_postgres_data`.
- Configuración de API y desarrollo: `localhost:5433/divorcios_db`, usuario `divorcios_user`. No se reproducen credenciales.
- Se conectó por TCP a `127.0.0.1:5433` y se verificaron nombre de base, usuario, puerto interno 5432 y `system_identifier = 7691366052193407010`, igual al obtenido directamente en el contenedor auditado.
- Antes de migrar no había tablas de usuario ni migraciones aplicadas. Se verificó la cadena de 18 migraciones y la ausencia de diferencias entre modelo y snapshot.

La aplicación utilizó EF `Database.MigrateAsync()` sobre el mismo contexto y la conexión TCP ya abierta y verificada. No se utilizó `EnsureCreated`, no se reinició la base y no se cargaron datos. Se conservó el control de esquema sin datos de la migración de cierre.

## Resultado y comprobaciones posteriores

| Comprobación | Resultado |
| --- | --- |
| Historial | Las 18 migraciones del proyecto, en orden, sin faltantes ni adicionales. |
| Última migración | `20260930155844_CerrarModeloExpedientesYRespuestasVersionadas` |
| Migraciones pendientes | 0 |
| Tablas de aplicación | Las 37 tablas de `divorcios`, coincidentes por nombre con el modelo EF. |
| Registros | 0 en cada tabla de aplicación, incluidos los seis catálogos. |
| Cambios pendientes del modelo | Ninguno; comprobación EF y `dotnet-ef migrations has-pending-model-changes`. |
| Esquema local frente a la referencia validada | Idéntico: 358 columnas, 278 restricciones y 159 índices. |

El historial de EF es una tabla de infraestructura adicional a las 37 tablas de aplicación. La comparación de esquema utiliza la referencia del modelo de las pruebas aisladas anteriores y cubre columnas, defaults, nulabilidad, claves, CHECK e índices; no depende de OID ni del orden físico de columnas.

Después de probar los endpoints se volvieron a comprobar el historial y todos los conteos: las tablas siguen sin filas. No se borraron datos, volúmenes ni archivos de migración. No se modificó el modelo ni se generaron migraciones adicionales para Swagger o los catálogos.

## Evidencia y siguiente bloque

`Aplicacion_Desarrollo_Local.json` conserva la identidad no secreta, las 18 migraciones, el inventario completo y sus conteos. `VALIDACION_POSTGRESQL.md` documenta la validación aislada previa; sus afirmaciones sobre una base local sin esquema corresponden a esa etapa anterior.

Swagger y el primer bloque de consultas se documentan en `Divorcios.Api/Revision/CATALOGOS_Y_SWAGGER.md`. La carga de valores y las operaciones de escritura continúan pendientes. No hay aplicación automática de migraciones al iniciar la API.
