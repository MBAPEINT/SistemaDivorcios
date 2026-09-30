# Migración de cierre del modelo — alcance y validación

Preparada y validada el 30 de septiembre de 2026. La validación aislada está en VALIDACION_POSTGRESQL.md. Posteriormente, con autorización expresa y verificación TCP de la instancia, se aplicó la cadena completa de 18 migraciones sólo a la nueva base local; véase APLICACION_DESARROLLO_LOCAL.md. No se cargaron filas ni se borraron datos, volúmenes o migraciones.

## Archivos y punto de partida

- Migración: `../20260930155844_CerrarModeloExpedientesYRespuestasVersionadas.cs`.
- Modelo objetivo: su Designer y `../DivorciosDbContextModelSnapshot.cs`.
- SQL incremental: `20260930155844_CierreModelo_Revision.sql`.
- Punto de partida del SQL: `20260918005514_AgregarRegistroAuditoria`, última migración histórica anterior.

Se conservan todas las migraciones anteriores. Este SQL incremental supone el último esquema histórico anterior. Para la nueva base local, inicialmente sin tablas ni historial, se aplicó la cadena completa. La base de la otra PC no fue auditada ni modificada. Este archivo no es una migración inicial ni un script idempotente.

## Respuestas versionadas incorporadas

En `preregistro_version` y `expediente_version` se almacenan:

| Columna | Tipo | Significado |
| --- | --- | --- |
| tiene_hijos_mayores_situacion_especial | boolean nullable, sin default | Sí, no o respuesta pendiente sobre el supuesto relevante de hijos mayores; sin datos médicos. |
| requiere_representacion_a | boolean nullable, sin default | Respuesta declarada del cónyuge A. |
| requiere_representacion_b | boolean nullable, sin default | Respuesta declarada del cónyuge B. |

`null` no se sustituye por `false`. La respuesta afirmativa sobre hijos mayores exige `tiene_hijos` y `cantidad_hijos_mayores > 0`; cada tabla tiene su CHECK. No se infiere esa respuesta de la cantidad total.

Se eliminó `TieneRepresentacion` de las entidades y configuraciones actuales: no se crea una columna global duplicada. Un resumen futuro se deriva de A/B: true si alguna es true, false si ambas son false, null en los demás casos. Las respuestas no acreditan poderes ni crean registros de Representacion.

Los servicios de envío y formalización siguen pendientes: deben conservar la encuesta original y crear/copiar el snapshot oficial con sus respuestas, sin actualizar versiones históricas.

## Alcance acumulado y límite para bases con datos

La migración también recoge los cambios anteriores del modelo que aún no estaban representados en el snapshot: eliminación de Caso como raíz, relaciones con Expediente, versiones, contactos, asociaciones de archivos presentados/evaluados y restricciones ya revisadas de asistencia, pagos y auditoría.

Por eso el SQL contiene DROP de seis tablas antiguas y columnas del esquema anterior, además de creación de diez tablas y cambios de relaciones/índices. **No constituye una conversión que preserve registros del esquema anterior.** No se rellenan datos desconocidos ni se inventan respuestas, autores, vouchers o equivalencias de identificadores.

Antes de cualquier cambio estructural, tanto Up como Down ejecutan un bloque que comprueba las tablas del esquema divorcios, excluyendo la historia de migraciones. Toma bloqueos exclusivos mantenidos por la transacción y rechaza la operación si encuentra alguna fila, incluso en catálogos. Una base con datos necesita una estrategia específica de conversión; este borrador la protege deteniendo la operación. El control no borra ni vacía tablas.

Se corrigieron dos equivalencias inferidas automáticamente por EF: `documento.actuacion_administrativa_id` no se renombra a `creado_por_usuario_id`, y `cuenta_ciudadana.bloqueado_en` no se renombra a `ultimo_acceso_en`. Son retirada y creación de columnas distintas, bajo el mismo control de esquema sin datos. Los demás cambios de FK no implican correspondencias de registros antiguos.

No retirar el control ni ejecutar este SQL en una base existente como atajo. La aplicación autorizada ya terminó sólo en la nueva base local previamente vacía. Si otra base tiene datos, preparar una conversión que los conserve y acordar su ejecución específica; la autorización local no se extiende a la otra PC.

## Verificación previa del modelo y validación PostgreSQL posterior

- Compilación de la solución: 0 errores y 0 advertencias.
- EF confirma que el snapshot coincide con el modelo actual.
- Modelo: 37 entidades, 76 FK y 165 CHECK; sin propiedades sombra, FK duplicadas ni ciclos obligatorios.
- Respuestas nullable y sin defaults; ausencia del indicador global; prueba de versiones históricas sin modificación y snapshot oficial independiente.
- Reutilización de A/v1 y B/v1 entre encuestas y corrección posterior a A/v2; presentaciones, evaluaciones y FK de origen anteriores intactas.
- 972 combinaciones de CHECK de hijos y respuestas por cónyuge evaluadas en SQLite en memoria, conservando null.
- Sintaxis del SQL y del bloque PL/pgSQL revisada con un parser de PostgreSQL, sin ejecutar sentencias.

La validación aislada ejecutó toda la cadena en PostgreSQL 17.11 y comprobó igualdad con el modelo y rechazos Up/Down sobre datos preservados. Después se aplicó la cadena autorizada a la nueva base local: 18 migraciones, 37 tablas sin filas y esquema idéntico a esa referencia. EF no detecta cambios pendientes. No constituye una conversión de una base antigua con datos; los servicios del trámite siguen pendientes.

Patrimonio, entrega al ciudadano, referencia de segunda solicitud y separación de naturaleza/calendario de plazos siguen pendientes. No se asignaron los intervalos ambiguos de 10 días.
