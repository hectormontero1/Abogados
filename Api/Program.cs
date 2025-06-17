using System.Text;
using Domain.Models;
using Domain.Repositorios;
using Infrastructure.Data;
using Infrastructure.NewFolder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Web;
using Qdrant.Client;
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
    Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2XFhhQlJHfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTH5VdkNiWnxXdHBQRGBUWkZ/");

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
    builder.Configuration.AddEnvironmentVariables();
    builder.Services.AddSingleton(_ => new OpenAI.OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY")));
    builder.Services.AddSingleton(_ => new QdrantClient("http://localhost", 6333));
    //builder.Services.AddSingleton(RagController);
    builder.Services.AddScoped<RagService>();
    builder.Services.AddHttpClient("ConFirmaLenta", client =>
    {
        client.Timeout = TimeSpan.FromMinutes(5);
    });
    var app = builder.Build();
    app.UseCors("ApiCorsPolicy");
    // Configure the HTTP request pipeline. 098ad2c8-6dfb-4a8d-955e-9f6f5425ad32|KcAPfQjYbtaQl4z9uPAj7teWGt-d7yNjqXlGBXQCwzRWtz_g7TVSoA
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