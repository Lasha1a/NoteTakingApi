using Microsoft.AspNetCore.Identity;
using NoteTaking.Api.Common.models;
using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using Serilog;

namespace NoteTaking.Api.Features.Auth;



public static class RegisterUser
{
    //request and response "Dtos"
    public record Request(string Email, string Password);

    public record Response(Guid Id, string Email);

    //endpoint mapping
    public static void Map(IEndpointRouteBuilder app) =>

        app.MapPost("/auth/register", Handle)
            .WithName(nameof(RegisterUser))
            .WithOpenApi();

    //endpoint handler
    static async Task<IResult> Handle(
        Request request,
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        CancellationToken ct)
    {
        var exists = await db.Users
            .AnyAsync(u => u.Email == request.Email, ct); //check if email already exists

        if (exists)
        {
            Log.Warning("Registration attempt with existing email: {Email}", request.Email);

            return Results.Conflict("Email already exists");
        }

        var user = new User //create new user
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password); //hash the password before saving to db

        db.Users.Add(user);
        await db.SaveChangesAsync(ct); //save the new user to the database

        Log.Information(
            "User {UserId} registered successfully with email {Email}",
             user.Id,
             user.Email
        );

        return Results.Ok(new Response(user.Id, user.Email)); //return the created user's id and email as a response
    }

}


