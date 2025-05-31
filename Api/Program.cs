using System.Text;
using Domain.Models;
using Domain.Repositorios;
using Infrastructure.NewFolder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
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
    var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>();

    builder.Services.AddCors(options => options.AddPolicy("ApiCorsPolicy", builder =>
    {
        builder.WithOrigins(allowedOrigins!).AllowAnyMethod().AllowAnyHeader();
    })); ;
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
    });
        GlobalDiagnosticsContext.Set("Application", " SISTEMA DELTA ENCUESTA 1.0");
    GlobalDiagnosticsContext.Set("Version", "1.0");
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.SaveToken = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
    builder.Services.AddControllers()
   .AddNewtonsoftJson(options =>
      options.SerializerSettings.ReferenceLoopHandling =
        Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    builder.Services
        .AddControllers()
        .AddNewtonsoftJson(options =>
        {
            // don't serialize with CamelCase (see https://github.com/aspnet/Announcements/issues/194)
            options.SerializerSettings.ContractResolver = new DefaultContractResolver();
        });
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
    app.UseCors("ApiCorsPolicy");
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    //app.UseAuthentication();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthorization();
    app.MapFallbackToFile("index.html");
    //app.UseCors();
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