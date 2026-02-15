using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;

namespace NoteTaking.Api.Features.Auth;

public class RefreshAccessToken
{
    //request and response records for the endpoint
    public record Request(string RefreshToken);

    public record Response(string AccessToken, string RefreshToken);

    public static void Map(IEndpointRouteBuilder app) => // mapping the endpoint to the app
        app.MapPost("/auth/refresh", Handle)
            .WithName(nameof(RefreshAccessToken))
            .WithOpenApi();

    static async Task<IResult> Handle( //endpoint handler
        Request request,
        AppDbContext db,
        TokenService tokenService,
        CancellationToken ct)
    {
        var token = await db.RefreshTokens //find the refresh token in the database and include the related user
            .Include(r => r.User)
            .SingleOrDefaultAsync(r =>
                r.Token == request.RefreshToken &&
                !r.IsRevoked &&
                r.ExpiresAt > DateTime.UtcNow,
                ct
            );

        if (token is null)
            return Results.Unauthorized();

        
        token.IsRevoked = true; //revoke the old refresh token to prevent reuse

        var newRefreshToken =
            tokenService.GenerateRefreshToken(token.UserId); //generate a new refresh token for the user

        db.RefreshTokens.Add(newRefreshToken);

        var accessToken =
            tokenService.GenerateAccessToken(token.User); //generate a new access token for the user

        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response( //return the new access token and refresh token in the response
            accessToken,
            newRefreshToken.Token
        ));
    }
}
