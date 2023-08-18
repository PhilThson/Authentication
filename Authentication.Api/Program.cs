using System.Text.Json.Serialization;
using Authentication.Api.Extensions;
using Authentication.Core.Constants;
using Authentication.Domain.DTOs;
using Authentication.Domain.Interfaces.Services;
using Authentication.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o
        .JsonSerializerOptions.DefaultIgnoreCondition
            = JsonIgnoreCondition.WhenWritingNull);

builder.Services.EnableCors();
builder.Services.AddTokenAuthentication(builder.Configuration);
builder.Services.AddTokenAuthorizationPolicy();

builder.Services.AddDbContext<AuthDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("Auth"));
});

builder.Services.AddServices();
builder.Services.AddSettings(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AuthConstants.CorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/weatherforecast", () => { })
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/authenticate", async (AuthenticateRequestDto model) =>
{
    using var scope = app.Services.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var result = await userService.Authenticate(model);
    return Results.Ok(result);
})
.WithName("Authenticate")
.WithOpenApi()
.AllowAnonymous();

app.MapPost("/refreshToken", async (string refreshToken) =>
{
    using var scope = app.Services.CreateScope();
    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    var result = await userService.RefreshToken(refreshToken);
    return Results.Ok(result);
})
.WithName("RefreshToken")
.WithOpenApi()
.AllowAnonymous();

app.MapGet("/whoAmI", async ctx =>
{
    ctx.Response.StatusCode = 200;
    await ctx.Response
        .WriteAsync(ctx.User?.Claims
            .FirstOrDefault(x => x.Type == AuthConstants.UserIdClaim)
            ?.Value);
})
.WithName("User Info")
.WithOpenApi()
.RequireAuthorization(AuthConstants.TokenPolicy);

app.Run();
