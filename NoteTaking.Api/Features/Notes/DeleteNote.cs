using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using Serilog;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public class DeleteNote
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/notes/{id:guid}", Handle)
            .RequireAuthorization()
            .WithName(nameof(DeleteNote))
            .WithOpenApi();


    static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) // try to find the claim with type NameIdentifier
                         ?? user.FindFirst("sub");

        if (userIdClaim is null)
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        var note = await db.Notes // try to find the note with the given id, userId 
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                n.userId == userId &&
                n.IsDeleted == "false",
                ct);

        if (note is null)
            return Results.NotFound();

        note.IsDeleted = "true";
        note.UpdatedAt = DateTime.UtcNow; //soft delete

        await db.SaveChangesAsync(ct);

        Log.Information("Note with id {NoteId} deleted by user {UserId}", note.Id, userId);

        return Results.NoContent();
    }
}
