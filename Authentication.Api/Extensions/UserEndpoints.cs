﻿using System.Security.Claims;
using Authentication.Core.Constants;
using Authentication.Core.Exceptions;
using Authentication.Core.Settings;
using Authentication.Domain.DTOs.Common;
using Authentication.Domain.DTOs.Create;
using Authentication.Domain.DTOs.Read;
using Authentication.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Authentication.Api.Extensions;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        app.MapGet("/users", GetAllUsers)
            .WithName("GetAllUsers")
            .WithOpenApi()
            .RequireAuthorization(AuthConstants.TokenPolicy);

        app.MapGet("/users/{id}", GetUserById)
            .WithName("GetUserById")
            .WithOpenApi()
            .RequireAuthorization(AuthConstants.TokenPolicy);

        app.MapPost("/register", RegisterUser)
            .WithName("RegisterUser")
            .WithOpenApi()
            .RequireAuthorization(AuthConstants.ApiKeyAuthPolicy);

        app.MapPost("/authenticate", Authenticate)
            .WithName("Authenticate")
            .WithOpenApi()
            .AllowAnonymous();

        app.MapPost("/refreshToken", RefreshToken)
            .WithName("RefreshToken")
            .WithOpenApi()
            .AllowAnonymous();

        app.MapGet("/whoAmI", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithOpenApi()
            .RequireAuthorization(AuthConstants.TokenPolicy);
    }

    private static async Task<Ok<IEnumerable<ReadSimpleUserDto>>>
        GetAllUsers(string? ids, IUserService userService)
    {
        var users = await userService.GetAll(ids);
        return TypedResults.Ok(users);
    }

    private static async Task<Results<Ok<ReadUserDto>, NotFound>>
        GetUserById(int id, IUserService userService)
    {
        var readUserDto = await userService.GetById(id);
        return TypedResults.Ok(readUserDto);
    }

    private static async Task<Results<Created<ReadUserDto>, BadRequest>>
        RegisterUser(CreateUserDto createUserDto, IUserService userService)
    {
        var readUserDto = await userService.Register(createUserDto);
        return TypedResults.Created(nameof(GetUserById), readUserDto);
    }

    private static async Task<Results<Ok<ReadAuthenticationDto>, NotFound, BadRequest>>
        Authenticate([FromBody] AuthenticateRequestDto authenticateDto, IUserService userService, 
            IOptionsMonitor<CookieSettings> cookieOptions, HttpContext httpContext)
    {
        var responseDto = await userService.Authenticate(authenticateDto);
        httpContext.Response.Cookies.Append(
            "RefreshToken",
            responseDto.RefreshToken, 
            GetCookieOptions(cookieOptions.CurrentValue, responseDto.RefreshTokenExpirationTimeDays));
        
        return TypedResults.Ok(new ReadAuthenticationDto()
        {
            JwtToken = responseDto.JwtToken
        });
    }

    private static async Task<Results<Ok<ReadAuthenticationDto>, NotFound, BadRequest>>
        RefreshToken([FromServices] IUserService userService, HttpContext httpContext,
            IOptionsMonitor<CookieSettings> cookieOptions)
    {
        if (!httpContext.Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
        {
            throw new DataValidationException("No refresh token provided");
        }
        var responseDto = await userService.RefreshToken(refreshToken);
        httpContext.Response.Cookies.Append(
            "RefreshToken", 
            responseDto.RefreshToken, 
            GetCookieOptions(cookieOptions.CurrentValue, responseDto.RefreshTokenExpirationTimeDays));
        
        return TypedResults.Ok(new ReadAuthenticationDto()
        {
            JwtToken = responseDto.JwtToken
        });
    }

    private static IResult
        GetCurrentUser(HttpContext httpContext)
    {
        var id = httpContext.User.FindFirst(c => c.Type == AuthConstants.UserIdClaim)?.Value;
        var name = httpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Results.Ok(new { Id = id, Name = name });
    }

    private static CookieOptions GetCookieOptions(CookieSettings settings, int refreshTokenExpirationTimeDays) => 
        new()
        {
            HttpOnly = settings.HttpOnly,
            IsEssential = settings.IsEssential,
            Secure = settings.Secure,
            SameSite = SameSiteMode.Unspecified,
            Expires = DateTimeOffset.UtcNow.AddDays(refreshTokenExpirationTimeDays),
            MaxAge = TimeSpan.FromDays(refreshTokenExpirationTimeDays),
            Domain = settings.Domain
        };
}