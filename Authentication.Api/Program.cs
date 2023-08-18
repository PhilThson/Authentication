using System.Text.Json.Serialization;
using Authentication.Api.Extensions;
using Authentication.Core.Constants;
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

app.MapGet("/token", async ctx =>
{
    ctx.Response.StatusCode = 200;
    await ctx.Response
        .WriteAsync(ctx.User?.Claims
            .FirstOrDefault(x => x.Type == AuthConstants.UserIdClaim)
            ?.Value);
})
.RequireAuthorization(AuthConstants.TokenPolicy);

app.Run();
