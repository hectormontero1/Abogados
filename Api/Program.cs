using Domain.Models;
using Domain.Repositorios;
using Infrastructure.NewFolder;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();  // Elimina los proveedores predeterminados de log
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Host.UseNLog(); // <-- Esto permite que lea desde appsettings.json

    //NLog.Common.InternalLogger.LogToConsole = true;
    //NLog.Common.InternalLogger.LogLevel = NLog.LogLevel.Debug;

    builder.Services.AddDbContext<AbogadosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
builder.Services.AddScoped<IAbogadoRepository, AbogadoRepository>();
    builder.Services.AddScoped<ICasoRepository, CasoRepository>();
    builder.Services.AddScoped<ICasoService, CasoService>();
    builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
}
catch (Exception ex)
{
    
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}