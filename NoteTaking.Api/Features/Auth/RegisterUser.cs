using Microsoft.AspNetCore.Identity;
using NoteTaking.Api.Common.models;
using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;

namespace NoteTaking.Api.Features.Auth;



public static class RegisterUser
{
    public record Request(string Email, string Password);

    public record Response(Guid Id, string Email);

    public static void Map(IEndpointRouteBuilder app) =>

        app.MapGet("/auth/register", Handle)
            .WithName(nameof(RegisterUser))
            .WithOpenApi();

    static async Task<IResult> Handle(
        Request request,
        AppDbContext db,
        IPasswordHasher<User> passwordHasher,
        CancellationToken ct)
    {
        var exists = await db.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (exists )
            return Results.BadRequest("Email alrdy exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(user.Id, user.Email));
    }

}


