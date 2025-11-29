using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using Serilog;
using AuthForge.Api.Common.Extensions;
using AuthForge.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/authforge-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


builder.Services.AddDatabases(builder.Environment);
builder.Services.AddApplicationServices(
    builder.Environment,
    builder.Configuration);

builder.Services.AddAuthForgeAuthentication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactCorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();


await app.EnsureConfigDatabaseAsync();


app.UseCors("ReactCorsPolicy");
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<SetupCheckMiddleware>();

app.UseMiddleware<JwtValidationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}.json"; });

    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("AuthForge API")
            .WithOpenApiRoutePattern("/openapi/v1.json");
    });
}


app.MapHealthChecks("/api/v1");
app.MapFeatureEndpoints();


app.Run();