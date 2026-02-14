using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public static class GetMyNotes //for paginations and search
{
    // record type for response
    public record class Response(
        Guid Id,
        string Title,
        string? Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );

    // endpoint mapping
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/notes", Handle)
            .RequireAuthorization()
            .WithName(nameof(GetMyNotes))
            .WithOpenApi();

    static async Task<IResult> Handle( // handler method
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) // try to find the claim with type "nameidentifier" first
            ?? user.FindFirst("sub");
        
        if (userIdClaim is null)
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        //query notes
        var notes = await db.Notes
            .Where(n => n.UserId == userId && n.IsDeleted == "false") // filter notes by user ID and not deleted
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new Response(
                n.Id,
                n.Title,
                n.Content,
                n.CreatedAt,
                n.UpdatedAt
            ))
            .ToListAsync(ct);

        return Results.Ok(notes);


    }

}
