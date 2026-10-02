# Guía de creación de endpoints y CRUD — SistemaDivorcios

Fecha de revisión: 2 de octubre de 2026.

El contrato propuesto de endpoints del módulo de prerregistro se comparte como documento Word por separado. Distingue rutas existentes, operaciones por desarrollar y decisiones pendientes.

Esta guía sirve para que el equipo agregue operaciones siguiendo la arquitectura actual: **API → Negocio → Datos**, con entidades compartidas de **Dominio**. Se utiliza TipoDocumento como ejemplo porque sus consultas y su creación ya están en el proyecto.

**Estado comprobado:** doce GET de catálogos y POST de tipos de documento. Paquete A: información, identidad y acceso ciudadano provisional. B: crear/listar/cabecera del PRE. C: cuatro rutas de encuesta versionada, contactos y generación parcial sustentada de requisitos. D: carga, corrección, reutilización y descarga exacta de archivos. PDF únicamente, máximo configurable de 10 MB. Contratos vigentes en secciones 14–18; matriz completa pendiente de validación jurídica. PUT/PATCH/DELETE de catálogos continúan como patrones educativos.

La guía inicial de catálogos se verificó por lectura y compilación. A/B/C/D y la matriz parcial tienen pruebas automatizadas en PostgreSQL temporal aislado. Autenticación interna, permisos administrativos, auditoría del acceso y envío/revisión/formalización siguen pendientes. No borrar datos, volúmenes ni migraciones de desarrollo.

## 1. Cómo se distribuye el trabajo entre las capas

~~~mermaid
flowchart LR
    C["Cliente o Swagger"] --> A["API: controlador"]
    A --> N["Negocio: servicio"]
    N --> D["Datos: repositorio"]
    D --> E["EF: DbContext"]
    E --> P["PostgreSQL"]
~~~

La petición entra al controlador. Este llama a un servicio. El servicio valida las reglas de la operación y llama al repositorio. El repositorio utiliza EF para consultar o guardar. En la respuesta, el servicio transforma las entidades en DTOs y el controlador entrega el estado HTTP.

| Capa | Responsabilidad | Ejemplo actual |
| --- | --- | --- |
| Dominio | Define las entidades y sus relaciones. | TipoDocumento, Documento. |
| Datos | Configura tablas, restricciones y relaciones; realiza consultas y guardados; reconoce errores de PostgreSQL. | DivorciosDbContext, CatalogosRepositorio. |
| Negocio | Aplica reglas de la operación, coordina persistencia y transforma entidades en DTOs. | CatalogosServicio. |
| API | Recibe parámetros y JSON, llama a Negocio y produce respuestas HTTP. | CatalogosController. |

Las referencias actuales son:

~~~text
Divorcios.Api     → Divorcios.Negocio
Divorcios.Negocio → Divorcios.Datos y Divorcios.Dominio
Divorcios.Datos   → Divorcios.Dominio
Divorcios.Dominio → ningún otro proyecto de la solución
~~~

La API registra Negocio en el arranque. La extensión de Negocio delega en Datos el registro de repositorios y DbContext. La API referencia directamente sólo Negocio.

Los controladores utilizan interfaces de servicio. Los servicios utilizan interfaces de repositorio. EF y Npgsql se utilizan en Datos. Las reglas del trámite pertenecen a Negocio.

## 2. Archivos reales que sirven como referencia

Los enlaces apuntan a archivos del repositorio mediante rutas relativas a este README. Se pueden abrir desde GitHub y consultar en cualquier equipo que conserve la estructura de la solución.

| Para aprender o implementar | Archivo de referencia | Qué revisar |
| --- | --- | --- |
| Entidad | [Divorcios.Dominio/Entidades/TipoDocumento.cs](Divorcios.Dominio/Entidades/TipoDocumento.cs) | Campos persistentes, identificador y navegaciones. |
| Configuración de la tabla | [Divorcios.Datos/Configuraciones/TipoDocumentoConfiguracion.cs](Divorcios.Datos/Configuraciones/TipoDocumentoConfiguracion.cs) | Longitudes, índice único, identidad y CHECK del origen. |
| DbContext | [Divorcios.Datos/Contexto/DivorciosDbContext.cs](Divorcios.Datos/Contexto/DivorciosDbContext.cs) | DbSet TiposDocumento y configuración del modelo. |
| DTO de salida | [Divorcios.Negocio/DTOs/Catalogos/TipoDocumentoDto.cs](Divorcios.Negocio/DTOs/Catalogos/TipoDocumentoDto.cs) | Campos que se exponen en la respuesta. |
| DTO de entrada | [Divorcios.Negocio/DTOs/Catalogos/CrearTipoDocumentoDto.cs](Divorcios.Negocio/DTOs/Catalogos/CrearTipoDocumentoDto.cs) | Required, StringLength y RegularExpression. |
| Contrato de Datos | [Divorcios.Datos/Interfaces/ICatalogosRepositorio.cs](Divorcios.Datos/Interfaces/ICatalogosRepositorio.cs) | Firmas de consulta, existencia y creación. |
| Implementación de Datos | [Divorcios.Datos/Repositorios/CatalogosRepositorio.cs](Divorcios.Datos/Repositorios/CatalogosRepositorio.cs) | AsNoTracking, AnyAsync, Add y SaveChangesAsync. |
| Error de persistencia | [Divorcios.Datos/Excepciones/CodigoDuplicadoPersistenciaException.cs](Divorcios.Datos/Excepciones/CodigoDuplicadoPersistenciaException.cs) | Error técnico reconocido y causa interna conservada. |
| Contrato de Negocio | [Divorcios.Negocio/Interfaces/ICatalogosServicio.cs](Divorcios.Negocio/Interfaces/ICatalogosServicio.cs) | DTOs de entrada y salida de la operación. |
| Implementación de Negocio | [Divorcios.Negocio/Servicios/CatalogosServicio.cs](Divorcios.Negocio/Servicios/CatalogosServicio.cs) | Validación, duplicados, creación y Mapear. |
| Conflicto de Negocio | [Divorcios.Negocio/Excepciones/ConflictoNegocioException.cs](Divorcios.Negocio/Excepciones/ConflictoNegocioException.cs) | Mensaje funcional y excepción interna opcional. |
| Controlador | [Divorcios.Api/Controllers/CatalogosController.cs](Divorcios.Api/Controllers/CatalogosController.cs) | GET, POST, rutas, 201, 400, 404 y 409. |
| Registro de Datos | [Divorcios.Datos/Extensiones/InyeccionDependencias.cs](Divorcios.Datos/Extensiones/InyeccionDependencias.cs) | AddDbContext y AddScoped del repositorio. |
| Registro de Negocio | [Divorcios.Negocio/Extensiones/InyeccionDependencias.cs](Divorcios.Negocio/Extensiones/InyeccionDependencias.cs) | AddScoped del servicio. |
| Arranque y Swagger | [Divorcios.Api/Program.cs](Divorcios.Api/Program.cs) | AddControllers, MapControllers, OpenAPI y Swagger UI. |
| Perfiles de ejecución | [Divorcios.Api/Properties/launchSettings.json](Divorcios.Api/Properties/launchSettings.json) | Puertos y apertura de Swagger. |

Se conserva el estilo de namespaces con llaves que ya utiliza el proyecto.

## 3. Orden exacto para agregar una operación

### Paso 1. Definir lo que hará el endpoint

Antes del código, acordar:

- Recurso y operación: consultar, crear, editar, desactivar o eliminar.
- Verbo y ruta.
- Parámetros de ruta, filtros y cuerpo JSON.
- Datos que se devolverán y estados HTTP.
- Campos editables, reglas de negocio y permisos.
- Efectos en registros relacionados, versiones e historial.

Ejemplo confirmado: crear un tipo de documento con código, nombre y origen. La base genera el identificador y el servicio lo crea activo.

No deducir que una tabla necesita las cuatro operaciones sólo porque existe.

### Paso 2. Revisar el modelo existente

Revisar entidad, configuración y DbContext. Confirmar tipos, nulabilidad, longitudes, índices y relaciones.

Agregar una operación sobre campos existentes normalmente no cambia el esquema. DTOs, métodos y controladores no requieren una migración por sí mismos. Si se descubre una necesidad de cambiar columnas o relaciones, tratarla como un bloque separado de modelado y persistencia.

### Paso 3. Definir los DTOs en Negocio

Crear o reutilizar:

~~~text
Divorcios.Negocio/DTOs/<Modulo>/
    <Recurso>Dto.cs
    Crear<Recurso>Dto.cs
    Actualizar<Recurso>Dto.cs
~~~

El DTO de salida representa lo que el cliente puede conocer. Los DTOs de entrada representan lo que puede solicitar modificar.

No recibir una entidad completa de EF en el controlador. Eso permitiría enviar identificadores, navegaciones u otros campos que no forman parte del contrato.

Los DTOs de creación y actualización pueden tener reglas diferentes; se definen según cada operación.

### Paso 4. Declarar las operaciones en la interfaz de Datos

Agregar las firmas en la interfaz del repositorio del módulo. Datos trabaja con entidades o resultados de persistencia, sin depender de los DTOs de Negocio.

~~~csharp
Task<TipoDocumento?> ObtenerTipoDocumentoAsync(
    int id, CancellationToken cancellationToken);

Task<bool> ExisteCodigoTipoDocumentoAsync(
    string codigo, CancellationToken cancellationToken);

Task<TipoDocumento> CrearTipoDocumentoAsync(
    TipoDocumento tipoDocumento,
    CancellationToken cancellationToken);
~~~

El signo de interrogación en TipoDocumento? permite devolver null cuando la consulta no encuentra el registro.

### Paso 5. Implementar el repositorio en Datos

Implementar las firmas utilizando el DbContext existente:

- Lectura: consultas LINQ y métodos asíncronos de EF.
- Creación: Add y SaveChangesAsync.
- Edición: cargar una entidad con seguimiento, modificar campos autorizados y guardar.
- Eliminación física permitida: Remove y SaveChangesAsync.
- Desactivación: modificar Activo y guardar.

Datos reconoce las restricciones técnicas esperadas. Debe comprobar el código de error y la restricción concreta para traducirlas; otros fallos continúan hacia el manejo general.

### Paso 6. Declarar la operación en la interfaz de Negocio

La interfaz del servicio utiliza DTOs:

~~~csharp
Task<TipoDocumentoDto> CrearTipoDocumentoAsync(
    CrearTipoDocumentoDto datos,
    CancellationToken cancellationToken);
~~~

La API conocerá esta interfaz. No llamará directamente al repositorio.

### Paso 7. Implementar el servicio de Negocio

En el servicio:

1. Validar la entrada.
2. Comprobar permisos, pertenencia y estado permitido cuando la operación los requiera.
3. Consultar información necesaria para las reglas.
4. Construir o modificar la entidad mediante campos explícitos.
5. Pedir al repositorio que guarde.
6. Traducir los errores técnicos esperados a errores de Negocio.
7. Convertir el resultado en el DTO de salida.

Para operaciones con varios cambios relacionados, coordinar también su atomicidad y auditoría. La pertenencia de una FK al mismo expediente no queda garantizada únicamente porque ese identificador exista.

### Paso 8. Agregar la acción en API

La acción:

1. Define verbo y ruta.
2. Recibe parámetros y DTO.
3. Llama al servicio.
4. Devuelve el estado HTTP y el DTO.
5. Traduce los errores conocidos a ProblemDetails o ValidationProblemDetails.

Agregar EndpointSummary y ProducesResponseType para que Swagger describa el contrato. Esas anotaciones documentan; no implementan validaciones ni permisos.

### Paso 9. Revisar el registro de dependencias

Si se crean un servicio y un repositorio nuevos, registrar sus implementaciones:

~~~csharp
// En la extensión de Datos:
servicios.AddScoped<IRecursoRepositorio, RecursoRepositorio>();

// En la extensión de Negocio:
servicios.AddScoped<IRecursoServicio, RecursoServicio>();
~~~

Son nombres de plantilla para un módulo nuevo; no existen actualmente en el proyecto.

Si sólo se agregan métodos a CatalogosServicio y CatalogosRepositorio, los registros existentes ya sirven. Las firmas nuevas sí deben agregarse a sus interfaces.

### Paso 10. Compilar y verificar el contrato

Compilar, revisar Swagger y comprobar los casos relevantes. No ejecutar escrituras en la base de desarrollo hasta que se acuerde una carga o prueba concreta.

## 4. GET: consultar datos

### 4.1. Listado existente

~~~http
GET /api/catalogos/tipos-documento
GET /api/catalogos/tipos-documento?activo=true
GET /api/catalogos/tipos-documento?activo=false
~~~

Orden de trabajo:

1. Reutilizar TipoDocumentoDto.
2. Agregar ListarTiposDocumentoAsync a la interfaz de Datos.
3. Implementar filtros y orden en Datos.
4. Agregar el método equivalente a la interfaz del servicio.
5. En Negocio, convertir cada entidad en DTO.
6. En API, recibir filtros y devolver Ok.
7. Comprobar lista vacía y filtros.

Consulta de referencia en Datos:

~~~csharp
return await contexto.TiposDocumento.AsNoTracking()
    .Where(x => !activo.HasValue || x.Activo == activo.Value)
    .OrderBy(x => x.Codigo)
    .ThenBy(x => x.TipoDocumentoId)
    .ToListAsync(cancellationToken);
~~~

AsNoTracking es apropiado para la lectura. El filtro nullable significa: sin filtro devuelve todos; true o false selecciona ese indicador. Activo no equivale automáticamente a vigencia o aplicabilidad.

Una lista sin resultados devuelve 200 con []. Para consultas que puedan crecer mucho, definir paginación y orden estable antes de cargar todos los registros.

### 4.2. Detalle existente

~~~http
GET /api/catalogos/tipos-documento/{id}
~~~

El repositorio devuelve una entidad o null. El servicio transforma la entidad en DTO. El controlador devuelve 200 o 404.

La acción actual valida que el identificador esté entre 1 y short.MaxValue. La ruta usa la restricción int: una URL que no satisface esa restricción no coincide y devuelve 404. Un número entero que coincide con la ruta pero falla Range produce 400.

**GET también valida entradas.** Valida identificadores y filtros. El DTO de salida no lleva Required para validar una petición porque se utiliza para la respuesta.

Guía de código: métodos ListarTiposDocumento y ObtenerTipoDocumento del controlador, y sus equivalentes Async en servicio y repositorio.

## 5. POST: crear un registro

Ruta existente:

~~~http
POST /api/catalogos/tipos-documento
~~~

Orden de archivos:

1. CrearTipoDocumentoDto.
2. ICatalogosRepositorio: ExisteCodigoTipoDocumentoAsync y CrearTipoDocumentoAsync.
3. CatalogosRepositorio: consulta de existencia y guardado.
4. CodigoDuplicadoPersistenciaException en Datos.
5. ICatalogosServicio: firma con DTO de entrada y salida.
6. CatalogosServicio: validación, normalización, duplicado, creación y mapeo.
7. ConflictoNegocioException en Negocio.
8. CatalogosController: acción POST y respuestas HTTP.

El DTO actual acepta Codigo, Nombre y OrigenCodigo. Las longitudes son 40, 160 y 20. El origen admite CIUDADANO, MUNICIPALIDAD o AMBOS porque esas opciones ya están en el CHECK del modelo. TipoDocumentoId lo genera PostgreSQL y el servicio establece Activo = true.

El servicio verifica las anotaciones mediante Validator.ValidateObject. [ApiController] también valida la petición antes de ejecutar la acción. Se conserva la validación en Negocio para otros posibles consumidores del servicio. [Referencia oficial de validación de controladores](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-10.0#automatic-http-400-responses).

El servicio elimina espacios exteriores del código y del nombre. No se ha definido conversión automática de códigos a mayúsculas ni una equivalencia entre códigos con distinta capitalización.

Add marca la entidad como nueva. SaveChangesAsync realiza el guardado. Si termina correctamente, EF incorpora el identificador generado.

La respuesta actual utiliza:

~~~csharp
return CreatedAtAction(
    nameof(ObtenerTipoDocumento),
    new { id = creado.TipoDocumentoId },
    creado);
~~~

Devuelve el DTO, estado 201 y cabecera Location para consultar el registro mediante el GET existente.

La consulta previa de existencia mejora el mensaje, pero el índice único sigue siendo la garantía ante concurrencia. El repositorio reconoce UniqueViolation sólo para ix_tipo_documento_codigo, retira del seguimiento la inserción fallida y lanza su excepción de persistencia. El servicio la transforma en un conflicto; la API devuelve 409.

Este POST no crea automáticamente requisitos oficiales, formatos ni documentos del trámite.

## 6. Edición: PUT y PATCH

**Estado: patrones pendientes de implementación.** Los archivos y métodos nuevos de esta sección son propuestas de contrato, no operaciones disponibles.

Editar exige definir primero qué campos se pueden modificar, qué actor puede hacerlo y qué consecuencias tiene para los registros que ya usan ese catálogo. El tipo de documento es referenciado por Documento; cambiar su significado requiere revisar el efecto histórico.

PUT representa el envío de la representación completa de los campos editables acordados. PATCH expresa un cambio parcial mediante un contrato definido. Un campo omitido no debe confundirse automáticamente con false o null. [Semántica de PUT](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.4), [semántica de PATCH](https://www.rfc-editor.org/rfc/rfc5789.html).

### 6.1. Pasos para una actualización con PUT

Ejemplo de ruta futura:

~~~http
PUT /api/catalogos/tipos-documento/{id}
~~~

1. En Negocio/DTOs/Catalogos, definir ActualizarTipoDocumentoDto con los campos realmente editables.
2. En Datos/Interfaces/ICatalogosRepositorio, declarar la carga para edición y el guardado.
3. En Datos/Repositorios/CatalogosRepositorio, implementar la carga con seguimiento.
4. En Negocio/Interfaces/ICatalogosServicio, declarar ActualizarTipoDocumentoAsync.
5. En Negocio/Servicios/CatalogosServicio, comprobar existencia y reglas, asignar campos explícitos, guardar y mapear.
6. En API/Controllers/CatalogosController, agregar HttpPut y devolver 200 con DTO o 404 si no existe.
7. Traducir validación a 400 y conflictos a 409.
8. Comprobar que los campos no editables y las relaciones históricas permanecen intactos.

Firmas orientativas de Datos:

~~~csharp
Task<TipoDocumento?> ObtenerTipoDocumentoParaEdicionAsync(
    int id, CancellationToken cancellationToken);

Task GuardarCambiosAsync(
    CancellationToken cancellationToken);
~~~

Carga orientativa del repositorio:

~~~csharp
public Task<TipoDocumento?> ObtenerTipoDocumentoParaEdicionAsync(
    int id, CancellationToken cancellationToken)
{
    return contexto.TiposDocumento.AsTracking()
        .SingleOrDefaultAsync(
            x => x.TipoDocumentoId == id,
            cancellationToken);
}
~~~

GuardarCambiosAsync utilizaría SaveChangesAsync sobre ese mismo contexto.

Firma orientativa del servicio:

~~~csharp
Task<TipoDocumentoDto?> ActualizarTipoDocumentoAsync(
    int id,
    ActualizarTipoDocumentoDto datos,
    CancellationToken cancellationToken);
~~~

El retorno nullable permite comunicar ausencia al controlador. ActualizarTipoDocumentoDto todavía debe diseñarse; no existe actualmente.

Algoritmo del servicio:

~~~text
Validar DTO
    → cargar entidad para edición
    → si no existe, devolver null
    → validar permiso y cambios admitidos
    → asignar sólo campos autorizados
    → guardar
    → devolver DTO
~~~

No construir una entidad completa desde un JSON y marcar todas sus propiedades como modificadas. Ese procedimiento podría sobrescribir identificadores, estados o valores que el cliente no debería controlar.

Si se permitirá editar el código, la comprobación de duplicados debe excluir el identificador actual. Reutilizar sin cambios el método ExisteCodigoTipoDocumentoAsync marcaría como duplicado el código del propio registro.

La política de campos editables y el control de concurrencia de edición siguen pendientes. No introducir un token de concurrencia o una nueva columna sin revisar su necesidad.

### 6.2. Ejemplo de cambio parcial: activar o desactivar

Para catálogos reutilizados, una operación específica sobre Activo puede conservar el registro y sus referencias. Debe acordarse su significado antes de implementarla.

Contrato educativo:

~~~http
PATCH /api/catalogos/tipos-documento/{id}/activo
~~~

Posible DTO futuro:

~~~csharp
using System.ComponentModel.DataAnnotations;

namespace Divorcios.Negocio.DTOs.Catalogos
{
    public sealed class CambiarActivoTipoDocumentoDto
    {
        [Required(ErrorMessage = "Debe indicar el valor de activo.")]
        public bool? Activo { get; set; }
    }
}
~~~

Se utiliza bool? para distinguir un valor omitido de false. Required acepta false y rechaza null. Tras validar, el servicio puede utilizar Activo.Value.

Este ejemplo describe un cuerpo JSON específico con un único campo. El formato y el contrato de PATCH se documentan expresamente; no requiere asumir que exista una implementación de JSON Patch.

El servicio cargaría el catálogo, validaría la operación, modificaría Activo y guardaría. Desactivar no borra documentos ni asociaciones históricas. La política de consultas debe aclarar cómo se muestran los inactivos; nuestros GET actuales ya permiten consultarlos.

## 7. DELETE: eliminar cuando el recurso lo permita

**Estado: patrón educativo, pendiente de implementación y de política de eliminación.** No ejecutar este ejemplo contra la base actual.

Ejemplo de ruta física futura:

~~~http
DELETE /api/catalogos/tipos-documento/{id}
~~~

### 7.1. Decidir eliminación física o desactivación

Eliminar físicamente retira la fila. Desactivar conserva la fila y cambia su disponibilidad para operaciones nuevas.

En este proyecto:

- Documento referencia a TipoDocumento con DeleteBehavior.Restrict.
- Un tipo utilizado no puede borrarse físicamente mientras conserve esa referencia.
- Las versiones, evaluaciones, auditoría e históricos se conservan; no se les aplica un CRUD de borrado libre.
- Una corrección documental crea nuevas versiones y mantiene los vínculos anteriores.

La regla para catálogos sin uso debe acordarse. La existencia de un método DELETE no autoriza borrar registros relacionados ni cambiar Restrict por Cascade.

Guía de la relación real: [Divorcios.Datos/Configuraciones/DocumentoConfiguracion.cs](Divorcios.Datos/Configuraciones/DocumentoConfiguracion.cs).

### 7.2. Orden de trabajo para una eliminación permitida

1. Definir en el contrato qué se elimina y quién tiene permiso.
2. Declarar en la interfaz de Datos la consulta de uso y la eliminación.
3. En Datos, consultar referencias y preparar Remove.
4. Declarar la operación en la interfaz de Negocio.
5. En Negocio, cargar el registro, comprobar permiso y reglas, y solicitar el borrado.
6. En API, agregar HttpDelete y convertir el resultado en 204, 404 o 409.
7. Reconocer en Datos las restricciones de referencia esperadas si aparecen al guardar.
8. Verificar que los registros relacionados no se alteren.

Consulta de referencia para este ejemplo:

~~~csharp
return contexto.Documentos.AnyAsync(
    x => x.TipoDocumentoId == id,
    cancellationToken);
~~~

Fragmento del repositorio, una vez que la eliminación haya sido admitida por Negocio:

~~~csharp
contexto.TiposDocumento.Remove(tipoDocumento);
await contexto.SaveChangesAsync(cancellationToken);
~~~

Remove prepara el borrado; SaveChangesAsync lo ejecuta. Estos fragmentos no constituyen un endpoint terminado.

Algoritmo de Negocio:

~~~text
Cargar registro
    → si no existe, comunicar ausencia
    → comprobar actor y política de eliminación
    → comprobar referencias
    → si el borrado no está permitido, lanzar conflicto
    → solicitar eliminación
~~~

Una referencia puede aparecer después de la consulta previa. La FK de PostgreSQL sigue siendo la garantía final. Reconocer la violación de la FK esperada, traducirla a un error de persistencia y después a un conflicto de Negocio. La excepción actual de código duplicado sólo cubre el índice único; no cubre errores de FK.

Contrato orientativo de respuesta: 204 sin cuerpo al eliminar, 404 al no encontrar y 409 cuando la regla o una referencia impiden hacerlo. Una repetición de DELETE puede recibir otro estado manteniendo el mismo efecto final; documentar el comportamiento elegido. [Referencia de DELETE](https://www.rfc-editor.org/rfc/rfc9110.html#section-9.3.5).

## 8. Validaciones y excepciones: dónde poner cada cosa

| Comprobación | Ubicación | Ejemplo |
| --- | --- | --- |
| Campo obligatorio o longitud | DTO de entrada en Negocio. | Required y StringLength. |
| Valor permitido del cuerpo | DTO; coherente con el modelo. | OrigenCodigo actual. |
| Forma y rango del identificador | Parámetro de API. | Restricción int y Range. |
| Regla de la operación | Servicio de Negocio. | Código duplicado, estado permitido, representación válida. |
| Pertenencia y autorización | Servicio/caso de uso, con actor autenticado; la API aplica además políticas cuando existan. | Documento perteneciente al expediente y permiso para consultarlo. |
| Índice único, FK o CHECK | Configuración y PostgreSQL; Datos reconoce errores esperados. | Código único y referencia documental. |
| Respuesta HTTP | API. | 400, 404 o 409. |

**Required no sustituye una regla de negocio.** Una cadena con longitud correcta puede no ser un número municipal válido. Una FK existente puede pertenecer a otro expediente.

### 8.1. Dos carpetas de excepciones con responsabilidades diferentes

~~~mermaid
flowchart LR
    P["Error reconocido de PostgreSQL"] --> D["Excepción de Datos"]
    D --> N["Excepción de Negocio"]
    N --> A["ProblemDetails en API"]
~~~

- Datos/Excepciones: describe un problema de persistencia reconocido.
- Negocio/Excepciones: describe por qué la operación solicitada no puede completarse.
- API: convierte ese resultado en una respuesta para el cliente.

Negocio ya depende de Datos. Referenciar Negocio desde Datos produciría una dependencia circular. Por eso el repositorio no lanza directamente ConflictoNegocioException.

No hace falta crear una clase distinta para cada frase de error. Usar tipos cuando el consumidor necesite distinguir tratamientos, como un conflicto esperado frente a un fallo técnico inesperado.

### 8.2. Ejemplo existente de traducción

En Datos, el catch combina UniqueViolation con el nombre ix_tipo_documento_codigo. No transforma todas las DbUpdateException en duplicados. SqlState y ConstraintName son propiedades de PostgresException. [Referencia Npgsql](https://www.npgsql.org/doc/api/Npgsql.PostgresException.html).

En Negocio, CodigoDuplicadoPersistenciaException se transforma en ConflictoNegocioException, conservando la causa interna.

En API, el POST captura ConflictoNegocioException y devuelve 409. Captura ValidationException para devolver ValidationProblemDetails.

Los errores inesperados continúan al manejo general configurado mediante AddProblemDetails y UseExceptionHandler. La API no devuelve SQL, credenciales, stack traces ni Exception.ToString como mensaje de negocio.

La ausencia en un GET se maneja actualmente con null y 404. Una lista vacía se maneja con [] y 200; no necesita una excepción.

## 9. Registro de dependencias y funciones de los archivos

Registro actual de Datos:

~~~csharp
servicios.AddScoped<ICatalogosRepositorio, CatalogosRepositorio>();
~~~

Registro actual de Negocio:

~~~csharp
servicios.AddScoped<ICatalogosServicio, CatalogosServicio>();
~~~

AddScoped registra una relación entre interfaz e implementación para el ámbito de la petición. La misma instancia scoped se reutiliza cuando se solicita de nuevo dentro de ese ámbito.

**Se registra la clase, no cada método.** Agregar un GET, POST, PUT o DELETE a estas clases no requiere repetir AddScoped. Sus métodos sí deben existir en ambas interfaces e implementaciones.

Los DTOs y excepciones que se crean con new no necesitan registrarse en DI. Los controladores se descubren con AddControllers y se publican con MapControllers.

Para un módulo nuevo, crear sus clases e interfaces y agregar sus registros a las extensiones de la capa correspondiente. No es necesario agregar una referencia de API a Datos para usar ese módulo.

## 10. Guardado, transacciones, historial y concurrencia

La creación actual de TipoDocumento usa un único SaveChangesAsync en su repositorio. Los cambios de una llamada a SaveChanges se ejecutan en una transacción cuando el proveedor la soporta. [Transacciones en EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions).

Una operación del trámite puede necesitar guardar expediente, participantes, versión e historial juntos. No asumir atomicidad entre varios SaveChanges independientes. Negocio debe coordinar la operación mediante una abstracción de persistencia/transacción de Datos. Esa coordinación general todavía no está implementada; se diseñará con el caso de uso concreto.

Las restricciones de BD protegen invariantes, pero una consulta previa no elimina las condiciones de concurrencia. El POST de tipos de documento ya traduce el rechazo del índice único. Edición, secuencias y estados requieren su estrategia correspondiente.

No reutilizar la actualización genérica para sobrescribir una encuesta enviada, una versión documental o una evaluación cerrada. Conservar las evidencias y agregar las versiones/correcciones que correspondan.

Un método de CRUD no debe aprobar un expediente, ratificar una audiencia, registrar un pago efectuado o cerrar una revisión por modificar libremente una entidad. Esas son operaciones del flujo con reglas propias.

## 11. Respuestas HTTP y Swagger

| Resultado | Estado habitual | Uso en esta guía |
| --- | --- | --- |
| Consulta o actualización con DTO | 200 | GET existentes; actualización propuesta. |
| Registro creado | 201 | POST existente, con Location. |
| Operación completada sin cuerpo | 204 | Eliminación propuesta. |
| Entrada inválida | 400 | DTOs, identificadores y filtros. |
| Falta de autenticación | 401 | Ya se aplica al paquete A y a la consulta de la sesión. |
| Falta de permiso | 403 | Aplicable cuando se definan políticas. |
| Recurso ausente | 404 | GET existente; edición/eliminación propuestas. |
| Conflicto esperado | 409 | Duplicado actual; conflictos de edición/eliminación futuros. |
| Fallo inesperado | 500 | Manejo general. |

No convertir todos los errores a 400 o 409. Identificar su causa y mantener un formato consistente.

Documentar 200/201 con application/json. Documentar ProblemDetails y ValidationProblemDetails con application/problem+json. Evitar una anotación global que fuerce application/json sobre las respuestas ProblemDetails.

Swagger actual:

~~~text
Perfil http:  http://localhost:5151/swagger
Perfil https: https://localhost:7243/swagger
Documento:    /openapi/v1.json
~~~

OpenAPI y Swagger sólo se publican en Development. Swagger describe y permite invocar los endpoints; no añade reglas, autenticación ni auditoría por sí mismo.

## 12. Comprobaciones antes de entregar un endpoint

1. Entender la operación y sus efectos.
2. Comprobar DTO de entrada y salida.
3. Confirmar los métodos de ambas interfaces y sus implementaciones.
4. Revisar que el controlador llame a Negocio y el servicio a Datos.
5. Confirmar validaciones, ausencia y errores esperados.
6. Revisar restricciones y concurrencia.
7. Confirmar registros DI si hay clases nuevas.
8. Compilar sin errores ni advertencias.
9. Comprobar ruta, verbos y formatos en OpenAPI/Swagger.
10. Ejecutar las pruebas autorizadas y documentar sus límites.

Comandos de compilación desde la carpeta de la solución:

~~~powershell
dotnet build SistemaDivorcios.slnx
dotnet run --project Divorcios.Api --launch-profile http
~~~

Casos que se deben cubrir cuando se autorice la preparación de datos:

| Operación | Casos |
| --- | --- |
| GET | Lista vacía/poblada; detalle existente/ausente; filtros; identificador inválido; acceso permitido. |
| POST | Entrada inválida; creación; identificador y Location; código duplicado; duplicado concurrente. |
| PUT/PATCH | Ausencia; validación; campos permitidos; histórico intacto; permisos; concurrencia. |
| DELETE/desactivación | Ausencia; operación permitida; referencias; efecto histórico; repetición y concurrencia. |

La autorización vigente no incluye cargar datos de prueba. Esta guía no la cambia. Las simulaciones en memoria no equivalen a probar el guardado real, las FK o la concurrencia real de PostgreSQL.

## 13. Lista rápida para repartir el trabajo del grupo

Para cada operación, dejar escrito:

~~~text
Operación y ruta:
Actor y permisos:
DTO de entrada:
DTO de salida:
Métodos nuevos en la interfaz de Datos:
Implementación de consultas/guardado:
Métodos nuevos en la interfaz de Negocio:
Reglas del servicio:
Excepciones de persistencia y de Negocio:
Acción del controlador:
Respuestas HTTP:
Registro DI, si hay clases nuevas:
Pruebas realizadas y pendientes:
Efecto en historial, documentos y referencias:
~~~

Trabajar por operaciones completas. El responsable debe revisar los archivos compartidos del módulo para no duplicar métodos, rutas o registros. Mantener los cambios del modelo y las migraciones como decisiones explícitas separadas del CRUD.

Esta guía permite continuar el código; no determina requisitos oficiales, tarifas, plazos, integraciones ni políticas funcionales que aún están pendientes.

## 14. Paquete A y acceso ciudadano provisional implementados

Se mantiene **API → Negocio → Datos**. La única referencia de proyecto de API sigue siendo Negocio. Dominio y las 18 migraciones no se modificaron. Se añadió `Divorcios.Pruebas` como proyecto de verificación; los cuatro proyectos de aplicación conservan sus responsabilidades.

| Método y ruta | Acceso | Resultado |
| --- | --- | --- |
| GET `/api/preregistro/informacion` | Público | Orientación confirmada, formatos configurados, política de archivos y pendientes. |
| POST `/api/acceso-ciudadano/sesion` | Público, limitado por IP | Valida DNI y sufijo de dirección; crea/reutiliza la cuenta y emite token. |
| GET `/api/acceso-ciudadano/sesion` | Bearer | Cuenta/persona de la sesión; no recibe un ID de cuenta arbitrario. |
| POST `/api/identidad/consultas-dni` | Bearer y permiso `identidad.consultar-dni` | Identidad verificada mediante caché vigente o proveedor municipal. |

### Decisión de acceso y límites

El usuario autorizó provisionalmente **DNI + últimos tres caracteres de la dirección RENIEC**, conforme a la propuesta municipal. Se retiran espacios exteriores, se normaliza Unicode y no se distinguen mayúsculas; los caracteres interiores y la puntuación se conservan. Se exige una dirección con al menos tres caracteres. Una dirección vacía o nula rechaza el acceso; no se permite introducir otra dirección para sustituirla.

Este mecanismo permite suplantación a quien conozca o adivine la dirección: **no es autenticación fuerte**. Los límites de intentos y el token reducen abusos, pero no corrigen esa debilidad. No acredita correo/celular, consentimiento del otro cónyuge ni poderes de representación. Su reemplazo y una eventual verificación de contacto quedan pendientes. `HabilitarDniDireccion` permite desactivarlo.

Defaults técnicos: token de 15 minutos; cinco fallos de una cuenta existente activan bloqueo de 15 minutos; login limitado a cinco solicitudes por IP cada cinco minutos; consulta de DNI limitada a diez solicitudes por cuenta por minuto. Token y bloqueo son configurables. Los contadores por IP/cuenta del middleware residen en cada proceso; el bloqueo de cuenta y la cuota de proveedor se guardan/controlan en PostgreSQL. Para despliegue con réplicas/proxy habrá que acordar los límites compartidos y el tratamiento de IP. Estos tiempos **no son plazos administrativos**.

El token identifica cuenta y persona. Se comprueban firma, emisor, audiencia, vencimiento, estado de cuenta y correspondencia con la persona en cada petición autenticada. No se recibe una cuenta desde el JSON como autoridad. Guardar el token en memoria del cliente y enviarlo en `Authorization: Bearer <token>`; no hay renovación automática ni revocación individual de tokens. El cierre local elimina el token del cliente; la expiración y el estado de la cuenta controlan su aceptación en el servidor. Usar HTTPS en despliegue.

### Flujo de los archivos nuevos

1. [InformacionPreregistroController](Divorcios.Api/Controllers/InformacionPreregistroController.cs) llama a [InformacionPreregistroServicio](Divorcios.Negocio/Servicios/InformacionPreregistroServicio.cs). Esta lectura sólo necesita configuración, no un repositorio ni consultas RENIEC.
2. [AccesoCiudadanoController](Divorcios.Api/Controllers/AccesoCiudadanoController.cs) recibe [IniciarSesionCiudadanaDto](Divorcios.Negocio/DTOs/Identidad/IniciarSesionCiudadanaDto.cs). [AccesoCiudadanoServicio](Divorcios.Negocio/Servicios/AccesoCiudadanoServicio.cs) consulta identidad, compara el sufijo y coordina intentos/cuenta mediante [AccesoCiudadanoRepositorio](Divorcios.Datos/Repositorios/AccesoCiudadanoRepositorio.cs). Sólo tras el acceso correcto, API emite el token mediante [EmisorSesionCiudadana](Divorcios.Api/Seguridad/EmisorSesionCiudadana.cs).
3. [IdentidadController](Divorcios.Api/Controllers/IdentidadController.cs) recibe [ConsultarDniDto](Divorcios.Negocio/DTOs/Identidad/ConsultarDniDto.cs). [IdentidadServicio](Divorcios.Negocio/Servicios/IdentidadServicio.cs) busca caché, comprueba cuota y coordina persistencia. [IdentidadRepositorio](Divorcios.Datos/Repositorios/IdentidadRepositorio.cs) consulta/guarda `ConsultaReniec` y reutiliza `Persona` por DNI. [ReniecProveedor](Divorcios.Datos/Integraciones/ReniecProveedor.cs) encapsula HTTP y reconoce el JSON municipal.
4. [ExcepcionesNegocioHandler](Divorcios.Api/Errores/ExcepcionesNegocioHandler.cs) traduce excepciones conocidas a ProblemDetails con `codigo` y `traceId`. Los errores inesperados mantienen la respuesta genérica de `UseExceptionHandler`; no se entregan SQL, contraseñas ni JSON bruto.

La consulta autenticada P02 no crea una cuenta para el DNI consultado y no devuelve su dirección. Los IDs nuevos `long` se devuelven como cadenas decimales; los catálogos conservan sus contratos numéricos.

### RENIEC: configuración y comportamientos comprobados

Campos reconocidos: `dni`, `prenombres`, `apPrimer`, `apSegundo`, `direccion`. Se exige coincidencia del DNI consultado, nombres/apellidos completos y longitudes compatibles con el modelo. Una respuesta con nombres nulos, JSON inválido, DNI distinto, error HTTP o cuerpo excesivo se registra como `ERROR`, sin Persona ni identidad verificada. La dirección puede faltar para consultar identidad, pero impide el acceso provisional.

El proveedor todavía no documentó una señal inequívoca de `NO_ENCONTRADO`. No se interpreta un 404 ni nombres nulos como ese resultado. Hasta confirmar esa señal, los resultados incompletos generan `503 RENIEC_RESPUESTA_NO_VERIFICABLE`. Los fallos se conservan y cuentan para la cuota; no se reutilizan como caché válida.

Se conserva cada consulta anterior. La prioridad es **buscar en BD antes de consultar RENIEC**. Se reutiliza `ENCONTRADO` asociado a Persona, sin fecha de caducidad o con expiración futura. Según la indicación posterior del usuario, el valor inicial `VigenciaCacheMinutos = null` significa **sin caducidad automática**: una identidad verificada guardada no vuelve a consumir consultas por el paso del tiempo. Si después se establece una vigencia positiva, se respeta su expiración; no se reutilizan errores ni datos incompletos. No hay refresco periódico. Esta política conserva datos potencialmente antiguos; su actualización controlada queda por definir.

El usuario corrigió el cupo municipal a **10 consultas diarias**, sustituyendo la cifra anterior. La aplicación admite presupuesto positivo de hasta diez y cuenta todos los intentos `API`, también fallidos, en una **ventana móvil de 24 horas**, compartida por sus instancias conectadas a la misma BD. Es una medida conservadora mientras no se confirme el horario real de reinicio del proveedor; no concede otro cupo simplemente al cambiar de fecha. Las lecturas de caché siguen disponibles cuando se agota. El bloqueo transaccional de PostgreSQL serializa llamadas externas para evitar duplicados y proteger el presupuesto. Se reserva el intento antes de HTTP y se conserva si el cliente cancela. Una falla de persistencia/caída del proceso o consumo de esas credenciales fuera de esta aplicación impide garantizar el saldo exacto del proveedor. No se hicieron llamadas reales durante las pruebas.

Timeout técnico configurable de 15 segundos, sin reintentos automáticos; cuerpo máximo de 16 KiB; redirecciones deshabilitadas. Los loggers del cliente HTTP están deshabilitados porque la API municipal recibe credenciales en parámetros. No registrar manualmente la URL completa ni el cuerpo de acceso.

**Estado local:** credenciales recibidas y clave aleatoria de firma en User Secrets del proyecto API, fuera del repositorio. Desarrollo tiene `Reniec:Habilitado = true`, `LimiteDiario = 10` y `VigenciaCacheMinutos = null` conforme a la prioridad de reutilización indicada. La integración queda disponible para peticiones legítimas del usuario. Las pruebas sustituyen el proveedor por simulaciones; no se consultaron DNI reales ni se cargaron personas ficticias en la BD de desarrollo.

Cada compañero configura su propio User Secrets o variables de entorno; esos secretos no viajan en Git. Claves: `Reniec:Usuario`, `Reniec:Clave`, `JwtCiudadano:ClaveBase64` (Base64 de al menos 32 bytes aleatorios). En variables de entorno se usan `Reniec__Usuario`, `Reniec__Clave`, `JwtCiudadano__ClaveBase64`. Nunca copiar valores reales en README, appsettings, pruebas ni commits. Sin firma configurada, el login devuelve `503 SESION_NO_CONFIGURADA` antes de crear una cuenta.

P01 devuelve `formatos: []` y `politicaArchivos: null` mientras no se configuren formatos confirmados y límites de archivos. Publica `pendientesConfiguracion` para distinguirlo de una política ilimitada. No se inventaron documentos oficiales, tarifas ni plazos.

### Revisar en Swagger

1. Iniciar la API con el perfil HTTP/HTTPS de Visual Studio o `dotnet run --project Divorcios.Api --launch-profile http`.
2. Abrir `/swagger`; probar GET `/api/preregistro/informacion`.
3. Ejecutar POST `/api/acceso-ciudadano/sesion` con un DNI real autorizado y `sufijoDireccion`. En este equipo los secretos están configurados; los demás deben establecer los suyos. No enviar `xxxxxxxx`, DNI ficticios ni adivinar direcciones: si faltan datos en BD, una consulta real consume el pequeño cupo municipal.
4. Copiar `accessToken` de la respuesta y pegar sólo el token en **Authorize**.
5. Probar GET de sesión y POST `/api/identidad/consultas-dni`. Sin sesión devuelve 401; sin permiso, 403; entrada inválida, 400; límite, 429; integración pendiente/incompleta, 503.

### Verificación automatizada

Ejecutar `dotnet test SistemaDivorcios.slnx`. Requiere Docker activo y la imagen `postgres:17-alpine`. [PostgresAislado](Divorcios.Pruebas/PostgresAislado.cs) crea un contenedor con nombre propio, puerto aleatorio en loopback y almacenamiento temporal `tmpfs`. Aplica las 18 migraciones exclusivamente allí y usa datos ficticios. Comprueba nombre/host/puerto para rechazar la base habitual de desarrollo. Al finalizar detiene exclusivamente su contenedor temporal; no utiliza ni elimina volúmenes de desarrollo.

Las pruebas cubren respuestas incompletas/HTTP, caché expirada, errores persistidos, cuota, cancelación, consultas concurrentes, creación/reutilización de cuenta, dirección nula, bloqueo, HTTP, tokens manipulados/vencidos/con audiencia incorrecta, correspondencia de persona, permisos, rate limit y Swagger. También comprueban las 18 migraciones, 37 tablas y ausencia de cambios EF del modelo.

**Paquetes B/C/D implementados:** secciones 15–17. La sección 18 documenta las correspondencias sustentadas y sus pendientes. Los formularios de P01 son referencias condicionales; no sustituyen la matriz ni obligan a todos los ciudadanos a presentar todos los formatos.

## 15. Paquete B — crear, listar y consultar el prerregistro

| Método y ruta | Resultado | Regla de acceso |
| --- | --- | --- |
| POST `/api/preregistros` | `201 PreregistroDetalleDto`, con `Location`. | El iniciador declarado corresponde a la persona de la sesión. |
| GET `/api/preregistros` | `200 PaginaDto<PreregistroResumenDto>`. | Sólo los expedientes donde participa la persona de la sesión, como iniciador u otro cónyuge. |
| GET `/api/preregistros/{preregistroId}` | `200 PreregistroDetalleDto`. | Participante; recurso inexistente/ajeno devuelve el mismo 404. |

Las tres rutas requieren Bearer. Autenticación/lectura interna sigue pendiente. La cuenta/persona autorizada se extrae de las claims validadas y se comprueba otra vez en Negocio; ningún ID suministrado como body/query decide el actor.

### C01: crear

Después del login y de obtener ambos `personaId` mediante P02 cuando haga falta:

```json
{
  "conyuges": [
    { "posicionCodigo": "A", "personaId": "101", "esIniciador": true },
    { "posicionCodigo": "B", "personaId": "102", "esIniciador": false }
  ]
}
```

Los IDs del ejemplo deben sustituirse por los recibidos de la API. Validación: exactamente dos personas distintas, A/B una vez cada una, respuesta `esIniciador` presente en ambos y exactamente uno verdadero. El iniciador puede estar en A o B; las posiciones no indican sexo. Ambas Personas deben existir con identidad verificada. Esta creación **no llama a RENIEC**, no crea Personas ni cuenta del otro cónyuge; utiliza los registros guardados por identidad.

[PreregistrosController](Divorcios.Api/Controllers/PreregistrosController.cs) recibe el DTO y llama a [PreregistrosServicio](Divorcios.Negocio/Servicios/PreregistrosServicio.cs). Negocio valida participantes/actor, genera un código digital aleatorio `PRE-...` de 26 caracteres y coordina la transacción mediante [PreregistrosRepositorio](Divorcios.Datos/Repositorios/PreregistrosRepositorio.cs). Se guardan Expediente, dos ExpedienteConyuge, Preregistro BORRADOR y RegistroAuditoria de creación. Si falla cualquiera, se revierte todo. El código digital no es el número de Mesa de Partes.

El JSON de creación rechaza campos adicionales, incluidos cuenta, número oficial, fechas/estados o versiones. Los IDs se validan como cadenas decimales positivas dentro de Int64. DNI, nombres, fechas de auditoría y actor no se aceptan como sustitutos de las personas identificadas y la sesión.

El resultado inicia `numeroExpediente`, envío/aprobación/bloqueo y referencias de versiones en `null`. No genera una encuesta con respuestas `false` por defecto, requisitos, archivos, estados formales ni plazos. La encuesta pertenece al paquete C. Se conserva `Observacion` en el historial de estados; este paquete no lo modifica ni inventa un catálogo/estado formal para llenarlo.

No se prohíbe otro prerregistro sólo por compartir DNI. **C01 todavía no es idempotente**: repetir una creación válida crea otro expediente digital; no habilitar reintentos automáticos en front y controlar el botón de envío. La clave de idempotencia y su conservación deben acordarse antes de implementar esa capacidad. No se añadió una tabla/columna de forma implícita.

### C02: listar

Filtros: `estadoCodigo` opcional (`BORRADOR`, `ENVIADO`, `OBSERVADO`, `APROBADO`, `CANCELADO`), `pagina` desde 1 y `tamanoPagina` de 1 a 100; valores iniciales 1/20. Orden por creación descendente y después ID descendente. Conteo/página se consultan en la misma instantánea PostgreSQL. La ausencia de resultados devuelve `items: []`, `total: 0`; una página posterior puede estar vacía con total positivo.

El listado contiene IDs/código, estado, número oficial cuando exista, fechas básicas, `estaBloqueado`, `soyIniciador` y nombres/posiciones de ambos cónyuges. No carga documentos, contactos, auditoría completa ni encuestas. Los IDs de las entidades se entregan como cadenas; el total/paginación son números.

### C03: cabecera y referencias

Cabecera: los mismos datos principales, fechas de aprobación/bloqueo, ambos participantes y referencias de versión/revisión. Se consultan en una instantánea consistente. No permite enumerar prerregistros de terceros; tener un ID no da acceso.

`versionTrabajoId` representa la última versión en un estado de trabajo habilitado, no la aprobada. `versionAprobadaId` procede de la revisión APROBADO cuya finalización coincide con la aprobación registrada. `versionEnviadaId` usa el evento de auditoría exacto del envío o una asociación de revisión inequívoca correspondiente a ese envío; no se deduce simplemente de la última encuesta. Cuando no se acredita una referencia, queda `null` y se señala en `referenciasPendientes`. Más de una revisión abierta se señala como ambigüedad y no anuncia preparación de encuesta.

Contrato a coordinar con el responsable del envío: guardar en la misma transacción `PREREGISTRO_ENVIADO`, recurso `PREREGISTRO_VERSION`, `RecursoId` de la versión, expediente y `RegistradoEn` igual al `EnviadoEn` del PRE. Revisiones/finalización deben conservar su versión real y la fecha aplicada a la aprobación. Es una convención sobre el modelo existente; envío/revisión aún no están implementados por este paquete.

`accionesDisponibles` incluye `CONSULTAR` y, para el iniciador en BORRADOR/OBSERVADO sin bloqueo/cierre ni revisión abierta, `CREAR_VERSION`, ya implementada por C. El otro cónyuge no recibe escritura. `accionesPendientesImplementacion` no repite esa acción. La reapertura de APROBADO sigue pendiente de política y no se implementó.

Las respuestas con datos de ciudadano usan `Cache-Control: no-store`. Errores: 400 por entrada inválida, 401 por sesión/cuenta inválida, 404 por PRE inexistente o ajeno, 409 ante conflicto reconocido. No se exponen errores SQL.

### Verificación y límites actuales

`dotnet test SistemaDivorcios.slnx` cubre creación con iniciador A/B, reutilización de Personas, ausencia de cuenta automática del segundo, validaciones, aislamiento entre participantes/terceros, filtros/paginación, fallo de auditoría con reversión real, referencias históricas exactas y HTTP 201/Location/400/401/404/Swagger. Carga datos **sólo en PostgreSQL temporal aislado**, compartido por las pruebas A/B/C. El modelo conserva 37 tablas y 18 migraciones; no se generaron ni aplicaron cambios sobre la base de desarrollo.

C ya guarda encuestas/contactos y consulta requisitos. D ya implementa carga, corrección, selección y descarga de archivos (apartado 17). P01 publica referencias municipales y PDF/10 MB. V01 genera sólo requisitos sustentados con catálogos/configuración disponibles; la matriz completa sigue pendiente. Véase sección 18.

## 16. Paquete C — encuestas y contactos históricos

Las cuatro rutas requieren Bearer. Sólo el iniciador guarda; ambos participantes consultan. La lectura interna espera autenticación/roles del otro bloque.

| Método y ruta | Resultado |
| --- | --- |
| POST `/api/preregistros/{preregistroId}/versiones` | 201 con Location a la versión exacta; encuesta y contactos en una transacción. |
| GET `/api/preregistros/{preregistroId}/versiones` | 200 con historia por número descendente; sin encuestas devuelve []. |
| GET `/api/preregistros/{preregistroId}/versiones/{versionId}` | 200 con encuesta exacta y contactos históricos. |
| GET `/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos` | 200 con estado de generación, pendientes e items de requisitos/evidencia existentes. |

### V01 y V03

[PreregistroVersionesController](Divorcios.Api/Controllers/PreregistroVersionesController.cs) recibe/entrega DTOs. [VersionesPreregistroServicio](Divorcios.Negocio/Servicios/VersionesPreregistroServicio.cs) valida sesión, iniciador, estado, precondición y coherencia; coordina snapshot/contactos/auditoría y transforma respuestas. [VersionesPreregistroRepositorio](Divorcios.Datos/Repositorios/VersionesPreregistroRepositorio.cs) maneja EF, transacciones y consultas. No hay acceso directo desde API a EF/SQL.

Ejemplo de primera encuesta, después de C01:

```json
{
  "versionBaseId": null,
  "motivoCambio": null,
  "encuesta": {
    "fechaMatrimonio": "2020-05-10",
    "matrimonioEnPorvenir": true,
    "ultimoDomicilioConyugalPorvenir": false,
    "domicilioConyugal": null,
    "tieneHijos": true,
    "cantidadHijosMenores": 0,
    "cantidadHijosMayores": 1,
    "tieneHijosMayoresSituacionEspecial": null,
    "tieneBienes": false,
    "tieneAcuerdoBienes": false,
    "requiereRepresentacionA": null,
    "requiereRepresentacionB": null,
    "observacionCiudadano": null
  },
  "contactos": [
    { "posicionCodigo": "A", "celular": null, "correo": null, "direccion": null },
    { "posicionCodigo": "B", "celular": null, "correo": null, "direccion": null }
  ]
}
```

Siguientes guardados: enviar `versionBaseId` con el ID de la última encuesta y `motivoCambio` no vacío. Cada POST contiene todos los valores, no un parche. El servidor asigna número, cuenta y fecha. Campos adicionales de actor/estado/fechas/requisitos se rechazan. Repetir una base superada devuelve 409; incluso una corrección sólo de archivos prepara una nueva encuesta con motivo.

Fecha, booleanos no nullable y cantidades deben estar presentes. Las cantidades van de 0 a 32767; sin hijos deben ser cero. Situación especial true requiere hijos y al menos un mayor. Sin bienes no se admite acuerdo true. Se conservan las tres respuestas nullable sin reemplazar null por false. A/B son independientes; no se acepta indicador global ni información médica. No se crean poderes por declarar representación. No se inventan categorías patrimoniales ni se implementa un motor jurídico de admisión.

Contactos: exactamente una entrada A y una B, aunque sus valores sean null. Se retiran espacios exteriores y cadenas vacías quedan null; correo/celular tienen comprobación básica de formato, sin verificarlos como canales. Cambiar un contacto cierra su intervalo y añade otro; eliminarlo sólo cierra, mantenerlo no lo duplica. Valores y versión de origen de registros anteriores se conservan. V03 usa la vigencia `[desde, hasta)`, incluyendo valores originados en una encuesta anterior que seguían vigentes. No reescribe Persona. Las marcas son estrictamente crecientes a precisión de microsegundos PostgreSQL, incluso con reloj fijo.

Una transacción READ COMMITTED bloquea filas Expediente y Preregistro antes de leer la última versión. Así dos guardados desde la misma base tienen un éxito y un 409. Primero se guardan cierres de contactos para liberar el índice único y después altas; fallo de auditoría revierte todos los pasos. Futuras escrituras del mismo agregado deben coordinarse con estos bloqueos.

Sólo BORRADOR/OBSERVADO permiten crear, sin bloqueo/cierre/formalización ni revisión abierta. OBSERVADO se conserva hasta el envío futuro; no borra/reemplaza su revisión. APROBADO no se reabre. Una encuesta es inmutable desde su creación. `editable` sólo indica permiso del iniciador para trabajar sobre los archivos de la última versión, mientras no se haya enviado ni revisado; no habilita PUT/PATCH de respuestas. Incluso una revisión finalizada congela esa versión: en OBSERVADO se crea primero una nueva V01. Versiones sustituidas no son versión de trabajo. Cabecera B y respuestas C usan la misma regla que D. Un ID de versión fuera del PRE devuelve 404.

### V02 y V04

V02 entrega ID, número, creación, motivo, si es de trabajo, revisiones vinculadas y generación de requisitos. `enviada=true` requiere evento PREREGISTRO_ENVIADO o revisión vinculada; `false` requiere un guardado auditado posterior al envío previo o sin envío; `null` conserva incertidumbre histórica. No usar la última encuesta como evidencia del envío.

V04 devuelve una **respuesta envolvente**:

```json
{
  "preregistroVersionId": "123",
  "estadoGeneracionCodigo": "PENDIENTE_CONFIGURACION",
  "pendientesConfiguracion": ["MATRIZ_RESPUESTA_REQUISITO", "TIPOS_Y_CANTIDAD_DOCUMENTAL"],
  "items": []
}
```

El ID es ilustrativo. El JSON muestra una generación sin configuración suficiente o una encuesta anterior. El bloque posterior autorizado implementa generación parcial de ramas sustentadas (sección 18): nuevas V01 pueden devolver GENERACION_PARCIAL con pendientes explícitos; sin catálogos/configuración permanecen PENDIENTE_CONFIGURACION. La auditoría PREREGISTRO_VERSION_CREADA conserva la determinación exacta de cada encuesta; una historia sin acreditación devuelve SIN_ACREDITAR. **Items vacío no demuestra completitud, permiso de envío ni aprobación.** Tener registros tampoco acredita que se aplicó una matriz completa. Quien implemente E/F debe conservar ese bloqueo. No se generan requisitos desde todos los catálogos activos ni desde supuestos ambiguos.

Cada ítem existente conserva aplica/obligatorio/estado e incluye los archivos de PreregistroRequisitoDocumento, por DocumentoVersion exacta; admite varios. No reemplaza la selección por la corrección más reciente, no mueve Documento.PreregistroRequisitoId y no cambia RevisionDetalleDocumento. Devuelve IDs y metadatos, nunca claves de almacenamiento. CARGADO no equivale a CONFORME. Catálogo desactivado no impide consultas históricas; su nombre/descripción son actuales. El campo determinacion de los requisitos generados conserva nombre al generar, versión de matriz, titulares, cobertura, tipos admitidos y fuentes históricos. En requisitos antiguos sin esa evidencia queda null y no se aplica retroactivamente la matriz actual.

MATRIZ_REQUISITOS_FUENTES_OFICIALES.docx y .md, junto al contexto fuera del repositorio, documentan la revisión por Ley 29227, reglamento, TUPA compartido y página/formularios oficiales. Distinguen correspondencias implementadas, propuestas y discrepancias jurídicas. MATRIZ_PROPUESTA_REQUISITOS conserva el antecedente de entrevistas.

### Verificación y siguiente bloque

Verificación: 58 pruebas correctas en la solución, incluidas 19 de C; compilación sin advertencias/errores, EF sin cambios del modelo. Base local auditada sólo en lectura: 37 tablas vacías y 18 migraciones. Pruebas C en PostgreSQL temporal: snapshots/nulls, contactos cambiados/eliminados/iguales con reloj fijo, permisos, pertenencia PRE–versión, versiones base/motivo, guardados concurrentes, reversión por fallo de auditoría, estados cerrados/revisión, OBSERVADO y evidencia reutilizada/corregida, historial sin acreditación y HTTP/Swagger. Catálogos/archivos ficticios de esas pruebas existen sólo en la base temporal; no representan documentos municipales.

Errores: 400 entrada inválida, 401 sesión inválida, 403 participante que no es iniciador al guardar, 404 recurso inexistente/ajeno, 409 estado/base en conflicto. Cache-Control no-store. Sin llamadas reales a RENIEC, cambios del modelo ni migraciones nuevas. D está implementado en el siguiente apartado; PDF/10 MB y matriz parcial se documentan en la sección 18. La prueba de 58 casos corresponde al cierre original de C, no al total vigente.

## 17. Paquete D — archivos presentados y evidencia histórica

Las cinco rutas están en Swagger. Se mantiene API → Negocio → Datos: [PreregistroArchivosController](Divorcios.Api/Controllers/PreregistroArchivosController.cs) sólo trata HTTP/formularios/streams; [ArchivosPreregistroServicio](Divorcios.Negocio/Servicios/ArchivosPreregistroServicio.cs) decide permisos, estado, tipo y selección; [ArchivosPreregistroRepositorio](Divorcios.Datos/Repositorios/ArchivosPreregistroRepositorio.cs) persiste con EF y [AlmacenamientoArchivosLocal](Divorcios.Datos/Almacenamiento/AlmacenamientoArchivosLocal.cs) guarda/lee bytes privados. IFormFile permanece en API. No cambia el modelo ni requiere otra migración.

| ID | Método y ruta | Entrada / resultado |
| --- | --- | --- |
| D01 | POST `/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/documentos` | Multipart: archivo, tipoDocumentoId, titulo. 201 DocumentoVersionDto + Location a D05. |
| D02 | POST `/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/documentos/{documentoId}/versiones` | Multipart: archivo, motivoCambio, documentoVersionBaseId. 201 nueva versión + Location. |
| D03 | PUT `/api/preregistros/{preregistroId}/versiones/{versionId}/requisitos/{requisitoId}/archivos-presentados` | JSON con documentoVersionIds. 200 PreregistroRequisitoDto. |
| D04 | GET `/api/preregistros/{preregistroId}/documentos` | pagina=1, tamanoPagina=20 (máximo 100), tipoDocumentoId opcional. 200 PaginaDto de documentos y versiones. |
| D05 | GET `/api/preregistros/{preregistroId}/documentos/{documentoId}/versiones/{documentoVersionId}/archivo` | 200 bytes exactos, descarga attachment, MIME comprobado, no-store y nosniff. |

### Permisos y versión de trabajo

Todas requieren Bearer y una cuenta ciudadana activa vinculada al participante. Ambos cónyuges leen; sólo el iniciador modifica. Un recurso inexistente o ajeno devuelve 404. La lectura interna necesita el bloque de autenticación/roles interno todavía pendiente.

D01–D03 sólo modifican requisitos de la última encuesta sin enviar/evaluar, en BORRADOR/OBSERVADO, sin bloqueos, formalización, cierre ni revisión abierta. Una encuesta revisada, aunque esté OBSERVADO, conserva sus selecciones: crear nueva V01, obtener los requisitos sustentados generados disponibles y seleccionar con D03. No se crean requisitos automáticamente en D ni se copian los de una encuesta anterior. Una lista vacía y CARGADO no acreditan completitud o conformidad.

### Cargar y corregir

D01 acepta exactamente un archivo por petición, tipo de catálogo positivo y título de 1–200 caracteres. Repetir la operación para varios documentos del requisito. El servidor fija autor, etapa PRERREGISTRO, estado VIGENTE, requisito de origen, número de versión, clave privada, MIME detectado, tamaño real y SHA-256. No acepta autor/estado/hash/rutas enviados por el ciudadano. No confiar en el Content-Type del archivo.

D02 conserva tipo, título y requisito de origen del documento. Requiere motivo no vacío de hasta 300 caracteres y **documentoVersionBaseId de la última versión lógica del documento**, consultada en D04. Este campo se añadió al contrato para impedir dos correcciones desde la misma base: una guarda y la otra devuelve 409. El documento debe estar presentado en ese requisito actual; si viene de una encuesta anterior, reutilizarlo primero con D03.

La corrección crea nuevos bytes, clave, hash y número. Sustituye únicamente la selección de ese Documento en ese requisito de trabajo y mantiene los demás archivos. Documento.PreregistroRequisitoId, otras presentaciones y RevisionDetalleDocumento conservan sus referencias exactas. D05 sigue descargando versiones históricas aunque exista una corrección.

DocumentoVersionDto entrega IDs, número, tipo, título, nombre seguro, MIME, tamaño, hash, fecha y urlDescarga; nunca clave o ruta física. El nombre recibido se reduce a su último segmento y se valida; no se utiliza para construir rutas de almacenamiento.

### Seleccionar y reutilizar

```json
{ "documentoVersionIds": ["801", "802"] }
```

IDs ilustrativos: usar los obtenidos en D04. PUT reemplaza el conjunto exacto de ese requisito. [] retira vínculos sin borrar documentos, bytes, origen ni evaluaciones. Repetir el mismo conjunto no añade vínculos ni auditorías. Se permiten varios documentos y como máximo una versión de cada Documento por requisito. El máximo de 100 IDs por operación es una protección técnica, no una cantidad documental oficial.

Cada asociación nueva exige mismo expediente/etapa, documento VIGENTE, requisito/tipo activos, origen CIUDADANO o AMBOS, correspondencia configurada y archivo disponible con hash/tamaño/formato correctos. Mantener una selección ya existente o retirarla no exige aprobar de nuevo esa correspondencia; no concede permiso para añadir otra. Las escrituras bloquean Expediente y Preregistro como C. PUT concurrentes se serializan y prevalece el último conjunto guardado; no hay combinación automática de selecciones.

D04 incluye todas las versiones del documento, referencias de presentación (versión de encuesta/requisito) y de evaluación (revisión/detalle/resultado). Incluye catálogos inactivos para consultas históricas. Que aparezca un archivo no significa que se admita para cualquier requisito. Asociaciones incoherentes con otro PRE generan 409, sin devolver referencias ajenas.

### Configuración y almacenamiento

En desarrollo permanecen explícitamente pendientes:

```json
{
  "Preregistro": {
    "Informacion": { "MaximoBytes": 10000000, "MimePermitidos": ["application/pdf"] },
    "Matriz": { "HabilitarGeneracionParcial": true }
  },
  "AlmacenamientoArchivos": { "DirectorioRaiz": null }
}
```

Política técnica aprobada: sólo PDF, máximo inicial 10 000 000 bytes por archivo (10 MB decimal), configurable en MaximoBytes. Se valida extensión, cantidad real de bytes y estructura mediante PdfPig 0.1.16 en modo estricto: documento no cifrado, al menos una página y análisis de todas sus páginas. Una cabecera %PDF- y un cierre %%EOF no bastan. JPG/PNG se rechazan en cargas nuevas; el adaptador conserva reconocimiento histórico para descargas previas. Sin política, D01/D02 devuelven 503 antes de leer el formulario. La solicitud multipart tiene 64 KiB adicionales exclusivamente para cabeceras/campos. El fragmento omite formatos y correspondencias: las cinco referencias municipales y diez correspondencias completas están en appsettings.Development.json; configurar el entorno de despliegue antes de habilitarlo.

TiposPorRequisito es configuración del servidor: cada entrada tiene RequisitoCodigo, Confirmada y TiposDocumentoCodigos, usando códigos existentes aprobados. Debe existir exactamente una correspondencia confirmada y no vacía por requisito. Si falta, D01/D02 y nuevas asociaciones D03 devuelven 503 TIPOS_REQUISITO_PENDIENTES. No la rellena el ciudadano ni se infiere desde todos los tipos activos. Confirmada expresa aprobación de esa correspondencia documental sustentada, no aprobación de toda la matriz ni conformidad de un archivo. V01 exige correspondencia exacta con todos los tipos alternativos; D respeta además la definición guardada con la encuesta, impidiendo ampliarla por un cambio posterior de configuración. No se cargaron catálogos o requisitos ficticios en desarrollo.

DirectorioRaiz null usa automáticamente `%LOCALAPPDATA%/SistemaDivorcios/archivos`, fuera del repositorio y de wwwroot. Una alternativa configurada debe ser una ruta absoluta privada del servidor. Los bytes usan claves aleatorias independientes del nombre, sin sobrescritura; se rechazan rutas fuera de la raíz y enlaces del sistema de archivos. /Divorcios.Api/ArchivosPrivados/ está ignorado como alternativa local. No exponer la carpeta mediante archivos estáticos.

La persistencia y el almacenamiento no comparten transacción: se guarda un archivo nuevo y luego sus filas/selección/auditoría dentro de la transacción del PRE. Si falla, sólo se descarta ese nuevo archivo cuando se confirma que ninguna DocumentoVersion conserva su clave. Ante una confirmación incierta o una consulta de comprobación fallida, se conserva y registra que necesita conciliación. No hay borrado automático de evidencia o archivos antiguos.

D05 verifica pertenencia PRE–documento–versión y vuelve a comprobar tamaño, SHA-256 y formato antes de entregar. Archivo ausente o alterado devuelve 503 ARCHIVO_NO_DISPONIBLE sin alterar su historia. El PDF se analiza estructuralmente sin ejecutar su contenido. El parser no es antivirus, comprobación de firmas ni revisión jurídica; puede rechazar archivos deteriorados que otros lectores reparan. La cancelación se comprueba entre páginas, no interrumpe internamente cada llamada síncrona del parser. Hash idéntico acredita mismos bytes. Retención, conciliación operativa, respaldo conjunto BD/archivos, permisos del almacenamiento y proveedor definitivo quedan para la configuración de despliegue.

Errores comunes: 400 metadatos/selección inválidos; 401 sesión inválida; 403 escritura por el otro cónyuge; 404 recurso ajeno/inexistente; 409 versión/base/estado en conflicto; 413 exceso de tamaño; 415 formato no admitido; 503 política/correspondencia/almacenamiento pendientes o indisponibles. Datos expresa fallos técnicos con ArchivoPersistenciaException; Negocio los transforma en errores de operación; API escribe ProblemDetails sin rutas ni errores SQL.

### Verificación y alcance terminado

86 pruebas correctas en la solución, incluidas 28 de D, con compilación sin advertencias/errores. [PaqueteDPruebas](Divorcios.Pruebas/PaqueteDPruebas.cs) usa PostgreSQL temporal y carpeta privada temporal: carga múltiple, descarga exacta para ambos, aislamiento entre expedientes, conservación de origen/presentaciones/evaluaciones tras corrección/reutilización, versión revisada congelada, retirada idempotente, concurrencia de correcciones, reversión por fallo de auditoría, archivos alterados/ausentes, entradas inválidas, tipos/orígenes, configuración pendiente y contratos HTTP/Swagger. No hace llamadas reales a RENIEC.

Con D están implementadas las rutas A–D de este reparto; 86 fue el resultado al cerrar D. El bloque posterior añade PDF/10 MB y generación parcial sustentada (sección 18). Permanecen pendientes el cierre jurídico de la matriz, carga revisada de catálogos, envío/validación/seguimiento, observaciones y revisión interna, formalización y sus permisos. No se declara terminado el módulo completo por tener estas rutas. Se conservan 37 tablas, 18 migraciones y el modelo existente; las pruebas no agregan datos a la base de desarrollo.


## 18. Matriz documental parcial sustentada y PDF

El usuario autorizó implementar las correspondencias claras después de documentarlas. Fuentes: [Ley 29227](https://www.leyes.congreso.gob.pe/Documentos/Leyes/29227.pdf), artículos 3–5; [DS 009-2008-JUS](https://www.muniate.gob.pe/wp-content/uploads/2026/02/DECRETO-SUPREMO-N%C2%B0-009-2008-JUS.pdf), artículos 4–6 y 15; [TUPA compartido](https://cdn.www.gob.pe/uploads/document/file/4999934/TUPA-MDEP.pdf?v=1692228069), páginas PDF 175–176; [página municipal](https://muniporvenir.gob.pe/servicios/tramite-de-divorcio/), requisitos generales y formularios. El Word de revisión contiene el sustento y discrepancias por fila. No se certifica que ese TUPA sea la última versión.

[MatrizRequisitosPreregistro](Divorcios.Negocio/Matrices/MatrizRequisitosPreregistro.cs) decide las ramas en Negocio. [GeneracionRequisitosServicio](Divorcios.Negocio/Servicios/GeneracionRequisitosServicio.cs) verifica configuración y catálogos activos mediante [GeneracionRequisitosRepositorio](Divorcios.Datos/Repositorios/GeneracionRequisitosRepositorio.cs). V01 integra encuesta/contactos/requisitos/auditoría en la misma transacción. API conserva el contrato de creación y V04 añade determinacion a cada requisito generado.

| Código interno de requisito | Condición implementada | Tipos internos admitidos (alternativas) |
| --- | --- | --- |
| PRE_SOLICITUD | Común; ambos | SOL_SEPARACION_CONVENCIONAL |
| PRE_MATRIMONIO | Común; un vínculo matrimonial | ACTA_PARTIDA_MATRIMONIO |
| PRE_NACIMIENTOS_MENORES | Menores > 0; cubrir cada menor | ACTA_PARTIDA_NACIMIENTO |
| PRE_REGIMEN_MENORES | Menores > 0; cubrir hijos y materias | SENTENCIA_REGIMEN_MENORES o ACTA_CONCILIACION_MENORES |
| PRE_DJ_SIN_MENORES | Menores = 0; declaración conjunta | DJ_SIN_HIJOS_MENORES |
| PRE_NACIMIENTOS_MAYORES_ESPECIAL | Mayores > 0 y situación especial true; sólo los comprendidos | ACTA_PARTIDA_NACIMIENTO |
| PRE_DJ_SIN_MAYORES_ESPECIAL | Situación especial false o cantidad mayores = 0 | DJ_SIN_MAYORES_SUPUESTO |
| PRE_PODER_A | Representación A true | PODER_ESPECIAL_INSCRITO |
| PRE_PODER_B | Representación B true, independiente de A | PODER_ESPECIAL_INSCRITO |
| PRE_DOMICILIO_CONYUGAL | Matrimonio fuera y último domicilio conyugal en El Porvenir | DJ_ULTIMO_DOMICILIO_CONYUGAL |

Son diez requisitos y nueve tipos distintos, identificadores del proyecto, no códigos oficiales. No se siembran catálogos dentro de una petición ni por migración. Deben existir activos; tipos de origen CIUDADANO o AMBOS. Si falta el requisito/tipo o su correspondencia exacta, se omite esa fila y queda CONFIGURACION_REQUISITO_<código>. La base de desarrollo continúa sin datos, por lo que el motor conserva PENDIENTE_CONFIGURACION hasta cargar catálogos revisados de forma autorizada. Pruebas crean valores sólo en PostgreSQL aislado.

Mayores ordinarios no activan el supuesto especial; respuesta null con mayores presentes queda pendiente sin declaración negativa. Representación null queda pendiente por cónyuge. No se deducen gananciales de TieneBienes/TieneAcuerdoBienes. La cantidad de hijos no determina cuántos PDFs exigir: varios documentos pueden cubrir uno o varios titulares; la cobertura individual se revisa. Los mayores en el supuesto especial mantienen pendiente su documentación jurídica, sin imponer reglas de menores, curatela/interdicción ni informes médicos por semejanza.

Discrepancias conservadas: copias de identidad en Ley/reglamento frente a exhibición en TUPA; declaraciones duplicadas del TUPA frente a formularios separados; menores/mayores mezclados en TUPA y marco posterior de capacidad jurídica; patrimonio genérico frente a sociedad de gananciales; alcance del anexo de domicilio cuando basta el matrimonio; hijos comunes frente a los de cada cónyuge y momento de acreditar pago. No se fijan tarifas/plazos ni se exige la solicitud de divorcio ulterior en separación inicial.

Cada nueva V01 guarda versión FUENTES_2026_10_01_PARCIAL_1, estado, pendientes y definición exacta de requisitos en PREREGISTRO_VERSION_CREADA. V04 lee esa evidencia histórica, nunca recalcula usando las respuestas con el motor actual. GENERACION_PARCIAL, PENDIENTE_CONFIGURACION y SIN_ACREDITAR **no permiten completar, enviar ni aprobar**, aunque todos los archivos estén cargados. E/F aún deben implementar sus operaciones y respetar el bloqueo. El guardado del snapshot no es firma criptográfica ni protección contra modificaciones administrativas directas de la BD.

Una configuración posterior no rellena encuestas anteriores. Crear nueva V01 y reutilizar versiones exactas por D03; D02 conserva relaciones anteriores y evaluaciones. HistorialEstadoExpediente.Observacion no cambia. Entrega al ciudadano, referencia de segunda solicitud y cálculo de plazos mantienen sus etapas pendientes.

Verificación vigente: 113 pruebas correctas con dotnet test. [MatrizRequisitosPruebas](Divorcios.Pruebas/MatrizRequisitosPruebas.cs) cubre nulls/ramas, representación independiente, alternativas, catálogos inactivos/orígenes, snapshot/configuración, rollback, reutilización/corrección, límite exacto de 10 MB y contrato HTTP. D añade rechazo de PDF simulado/truncado, cifrado, sin páginas y JPEG. Se comprueban 18 migraciones, 37 tablas y EF sin cambios de modelo; ninguna migración nueva o aplicada a desarrollo. Auditoría posterior sólo en lectura: cero registros de aplicación, incluidos catálogos.
