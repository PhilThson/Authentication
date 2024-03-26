using Microsoft.AspNetCore.Authentication;
using System.Text;
using Authentication.Core.Settings;
using Authentication.Core.Constants;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Authentication.Domain.Interfaces.Repositories;
using Authentication.Infrastructure.Repositories;
using Authentication.Services;
using Authentication.Domain.Interfaces.Services;
using Microsoft.OpenApi.Models;

namespace Authentication.Api.Extensions
{
    public static class ServiceExtensions
	{
        public static AuthenticationBuilder AddTokenAuthentication(this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();
            var jwtKey = configuration.GetSection(nameof(AuthConstants.JwtKey)).Value;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSettings?.Issuer,
                ValidAudience = jwtSettings?.Audience,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                //tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            };

            return services.AddAuthentication()
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
                    o.TokenValidationParameters = tokenValidationParameters);
        }

        public static IServiceCollection AddTokenAuthorizationPolicy(this IServiceCollection services)
        {
            return services.AddAuthorization(c =>
            {
                c.AddPolicy(AuthConstants.TokenPolicy, policy => policy
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireClaim(AuthConstants.UserIdClaim)
                    );
            });
        }

        public static void EnableCors(this IServiceCollection services, IConfiguration config)
        {
            var allowedOriginsString = config.GetValue<string>("AllowedOrigins");
            var allowedOrigins = Array.Empty<string>();
            if (!string.IsNullOrEmpty(allowedOriginsString))
            {
                allowedOrigins = allowedOriginsString.Split(",", StringSplitOptions.TrimEntries);
            }

            services.AddCors(options =>
            {
                options.AddPolicy(AuthConstants.CorsPolicy, builder => builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(allowedOrigins)
                    .AllowCredentials());
            });
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IJwtUtilsService, JwtUtilsService>();
            services.AddScoped<IUserService, UserService>();
        }

        public static void AddSettings(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication API", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });
        }
    }
}

