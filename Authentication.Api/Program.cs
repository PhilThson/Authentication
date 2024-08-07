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

builder.Services.EnableCors(builder.Configuration);
builder.Services.AddTokenAuthentication(builder.Configuration);
builder.Services.AddTokenAuthorizationPolicy();

builder.Services.AddDbContext<AuthDbContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("Auth"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
});

builder.Services.AddServices();
builder.Services.AddSettings(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandling();

app.UseCors(AuthConstants.CorsPolicy);

app.UseAuthentication();

app.UseAuthorization();

app.MapUserEndpoints();

app.Run();
