using Divorcios.Negocio.Extensiones;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration
    .GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión PostgreSQL.");

builder.Services.AgregarCapaNegocio(cadenaConexion);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
