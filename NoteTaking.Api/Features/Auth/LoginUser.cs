using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoteTaking.Api.Common.models;
using NoteTaking.Api.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Serilog;

namespace NoteTaking.Api.Features.Auth;

public static class LoginUser
{
    //request and response "Dtos" 
    public record Request(string Email, string Password);
    public record Response(string AccessToken, string RefreshToken);

    //endpoint mapping
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/auth/login", Handle)
        .WithName(nameof(LoginUser))
        .WithOpenApi();

    //endpoint handler
    static async Task<IResult> Handle(
        Request request,
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        TokenService tokenService,
        CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant(); //to avoid duplicated users with different cases or spaces

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct); //find the user by email in the database

        if (user is null)
        {
            Log.Warning("Login attempt with non-existing email: {Email}", request.Email);
            return Results.Unauthorized();
        }

        var result = passwordHasher.VerifyHashedPassword( //verify the provided password against the stored password hash
            user,
            user.PasswordHash,
            request.Password);

        if(result == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        //generating tokens from ServiceToken
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken(user.Id);

        //save the refresh token to the database
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        Log.Information("User {UserId} logged in successfully", user.Id);

        return Results.Ok(new Response(accessToken, refreshToken.Token));
    }
}
