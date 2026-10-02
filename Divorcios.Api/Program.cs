using Divorcios.Negocio.Extensiones;
using System.Globalization;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using Divorcios.Api.Errores;
using Divorcios.Api.Seguridad;
using Divorcios.Negocio.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration
    .GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión PostgreSQL.");

builder.Services.AgregarCapaNegocio(cadenaConexion);
builder.Services.ConfigurarPreregistro(builder.Configuration);
builder.Services.Configure<JwtCiudadanoOpciones>(builder.Configuration.GetSection(JwtCiudadanoOpciones.Seccion));
builder.Services.AddScoped<EmisorSesionCiudadana>();
builder.Services.AddScoped<LimiteCargaArchivoFiltro>();
builder.Services.AddExceptionHandler<ExcepcionesNegocioHandler>();

var jwt = builder.Configuration.GetSection(JwtCiudadanoOpciones.Seccion).Get<JwtCiudadanoOpciones>() ?? new();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opciones =>
{
    opciones.MapInboundClaims = false;
    opciones.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidIssuer = jwt.Emisor,
        ValidateAudience = true, ValidAudience = jwt.Audiencia,
        ValidateIssuerSigningKey = true,
        // Sin clave configurada no se emiten sesiones; el esquema permanece cerrado.
        IssuerSigningKey = new SymmetricSecurityKey(jwt.ObtenerClave() ?? RandomNumberGenerator.GetBytes(64)),
        ValidateLifetime = true, RequireExpirationTime = true, ClockSkew = TimeSpan.Zero,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], NameClaimType = "sub"
    };
    opciones.Events = new()
    {
        OnTokenValidated = async contexto =>
        {
            var principal = contexto.Principal!;
            if (!long.TryParse(principal.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
            {
                contexto.Fail("Sesión no válida.");
                return;
            }
            var servicio = contexto.HttpContext.RequestServices.GetRequiredService<IAccesoCiudadanoServicio>();
            var ciudadano = await servicio.ObtenerSesionAsync(id, contexto.HttpContext.RequestAborted);
            if (ciudadano is null || ciudadano.PersonaId != principal.FindFirst("persona_id")?.Value
                || principal.FindFirst("metodo_acceso")?.Value != "dni_direccion")
                contexto.Fail("Sesión no válida.");
        },
        OnChallenge = async contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.Headers.WWWAuthenticate = "Bearer";
            await ProblemasApi.EscribirAsync(contexto.HttpContext, 401, "SESION_REQUERIDA", "Se requiere una sesión ciudadana válida.");
        },
        OnForbidden = contexto => ProblemasApi.EscribirAsync(contexto.HttpContext, 403, "ACCESO_DENEGADO", "No tiene permiso para esta operación.")
    };
});
builder.Services.AddAuthorization(opciones => opciones.AddPolicy("ConsultarIdentidad",
    politica => politica.RequireAuthenticatedUser().RequireClaim("permiso", "identidad.consultar-dni")));
builder.Services.AddRateLimiter(opciones =>
{
    // Controles técnicos locales, independientes de la cuota real configurada del proveedor.
    opciones.AddPolicy("AccesoCiudadano", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "sin-ip", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 5, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
    opciones.AddPolicy("ConsultasIdentidad", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.User.FindFirst("sub")?.Value ?? "sin-sesion", _ => new FixedWindowRateLimiterOptions
        { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    opciones.OnRejected = async (contexto, cancellationToken) =>
    {
        if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var espera))
            contexto.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(espera.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        await ProblemasApi.EscribirAsync(contexto.HttpContext, 429, "DEMASIADOS_INTENTOS", "Se alcanzó el límite de solicitudes. Intente más tarde.");
    };
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, contexto, cancellationToken) =>
{
    documento.Components ??= new();
    documento.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    documento.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT",
        Description = "Obtenga accessToken en POST /api/acceso-ciudadano/sesion. Pegue sólo el token en Authorize."
    };
    foreach (var descripcion in contexto.DescriptionGroups.SelectMany(x => x.Items))
    {
        var metadatos = descripcion.ActionDescriptor.EndpointMetadata;
        if (!metadatos.OfType<IAuthorizeData>().Any() || metadatos.OfType<IAllowAnonymous>().Any()) continue;
        var ruta = "/" + descripcion.RelativePath?.Split('?')[0];
        if (!documento.Paths.TryGetValue(ruta, out var path) || path.Operations is null) continue;
        foreach (var operacion in path.Operations.Where(x => x.Key.Method.Equals(descripcion.HttpMethod, StringComparison.OrdinalIgnoreCase)))
            operacion.Value.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", documento)] = [] }];
    }
    return Task.CompletedTask;
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Sistema de Divorcios v1");
    });
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();

public partial class Program { }
