using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public class GetNoteById
{
    // record type for response
    public record class Response(
        Guid Id,
        string Title,
        string? Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        List<string> Tags);

    public static void Map(IEndpointRouteBuilder app) => // endpoint mapping
        app.MapGet("/notes/{id:guid}", Handle)
           .RequireAuthorization()
           .WithName(nameof(GetNoteById))
           .WithOpenApi();


    static async Task<IResult> Handle( // handler method
        Guid id,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) // try to find the claim with type "nameidentifier" 
                          ?? user.FindFirst("sub");

        if (userIdClaim is null)
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        var note = await db.Notes // query the note with the given id, userId and not deleted
            .Where(n =>
                n.Id == id &&
                n.UserId == userId &&
                n.IsDeleted == "false")
            .Select(n => new Response(
                n.Id,
                n.Title,
                n.Content,
                n.CreatedAt,
                n.UpdatedAt,
                n.NoteTags.Select(nt => nt.Tag.Name).ToList()
            ))
            .FirstOrDefaultAsync(ct);

        if (note is null)
            return Results.NotFound();

        return Results.Ok(note);
    }
}
