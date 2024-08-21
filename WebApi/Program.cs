using Application;
using Persistence;
using System.Reflection;
using WebApi.Extensions;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(configuration);
builder.Services.AddConfiguration(builder.Environment, configuration);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Documentacion API de Prueba para CSJ",
        Version = "v1",
        Description = "REST API de Prueba para la Aplicacion de Gestiones de la Corte   Suprema de Justicia"
    });
    c.EnableAnnotations();


    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory,xmlFile); 
    c.IncludeXmlComments(xmlPath);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicacion Iniciada Correctamente");

app.UseCors("UrlsGenericas");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHandlingMiddleware();

app.MapControllers();

app.Run();
