using AuthForge.Api.Endpoints;
using AuthForge.Api.Middleware;
using AuthForge.Application;
using AuthForge.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();  
app.UseMiddleware<ApplicationIdentificationMiddleware>();
app.UseAuthorization();


app.MapEndpoints();

app.Run();