using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoteTaking.Api.Common.models;
using NoteTaking.Api.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NoteTaking.Api.Features.Auth;

public static class LoginUser
{
    //request and response "Dtos" 
    public record Request(string Email, string Password);
    public record Response(string AccessToken);

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
        IConfiguration config,
        CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant(); //to avoid duplicated users with different cases or spaces

        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct); //find the user by email in the database

        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = passwordHasher.VerifyHashedPassword( //verify the provided password against the stored password hash
            user,
            user.PasswordHash,
            request.Password);

        if(result == PasswordVerificationResult.Failed)
            return Results.Unauthorized();

        //jwt token generation

        var jwt = config.GetSection("Jwt");

        var claims = new[] //claims to be included
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
        };
        
        var key = new SymmetricSecurityKey( //create a symmetric security
            Encoding.UTF8.GetBytes(jwt["Key"]!)
        );

        //create the JWT token with the specified claims, expiration time, and signing credentials
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                int.Parse(jwt["ExpiresMinutes"]!)
                ),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token); //serialize the token to a string

        return Results.Ok(new Response(tokenValue));
    }
}
