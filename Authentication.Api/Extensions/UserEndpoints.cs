using System.Security.Claims;
using Authentication.Core.Constants;
using Authentication.Domain.DTOs.Common;
using Authentication.Domain.DTOs.Create;
using Authentication.Domain.DTOs.Read;
using Authentication.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Http.HttpResults;

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
            .AllowAnonymous();

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

    public static async Task<Ok<IEnumerable<ReadUserDto>>>
        GetAllUsers(IUserService userService)
    {
        var users = await userService.GetAll();
        return TypedResults.Ok(users);
    }

    public static async Task<Results<Ok<ReadUserDto>, NotFound>>
        GetUserById(int id, IUserService userService)
	{
        var readUserDto = await userService.GetById(id);
        return TypedResults.Ok(readUserDto);
    }

    public static async Task<Results<Created<ReadUserDto>, BadRequest>>
        RegisterUser(CreateUserDto createUserDto, IUserService userService)
    {
        var readUserDto = await userService.Register(createUserDto);
        return TypedResults.Created(nameof(GetUserById), readUserDto);
    }

    public static async Task<Results<Ok<AuthenticateResponseDto>, NotFound, BadRequest>>
        Authenticate(AuthenticateRequestDto authenticateDto, IUserService userService)
    {
        var responseDto = await userService.Authenticate(authenticateDto);
        return TypedResults.Ok(responseDto);
    }

    public static async Task<Results<Ok<AuthenticateResponseDto>, NotFound, BadRequest>>
        RefreshToken(string refreshToken, IUserService userService)
    {
        var responseDto = await userService.RefreshToken(refreshToken);
        return TypedResults.Ok(responseDto);
    }

    public static IResult
        GetCurrentUser(HttpContext httpContext)
    {
        var id = httpContext.User.FindFirst(c => c.Type == AuthConstants.UserIdClaim)?.Value;
        var name = httpContext.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return Results.Ok(new { Id = id, Name = name });
    }
}