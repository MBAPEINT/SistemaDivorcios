using Divorcios.Datos.Extensiones;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration
    .GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión PostgreSQL.");

builder.Services.AgregarCapaDatos(cadenaConexion);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
