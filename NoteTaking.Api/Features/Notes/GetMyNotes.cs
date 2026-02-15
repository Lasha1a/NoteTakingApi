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
        DateTime? UpdatedAt,
        List<string> Tags
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
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Where(n => n.UserId == userId && n.IsDeleted != "true")
            .OrderByDescending(n => n.CreatedAt) // order by created date descending
            .Select(n => new Response(
                n.Id,
                n.Title,
                n.Content,
                n.CreatedAt,
                n.UpdatedAt,
                n.NoteTags.Select(nt => nt.Tag.Name).ToList()
                ))
            .ToListAsync(ct);

        return Results.Ok(notes);

    }
}
