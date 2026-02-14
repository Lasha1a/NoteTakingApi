using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using Serilog;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public class UpdateNote
{
    public record class Request(
        string Title,
        string? Content
    );

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/notes/{id:Guid}", Handle)
             .RequireAuthorization()
             .WithName(nameof(UpdateNote))
             .WithOpenApi();

    static async Task<IResult> Handle(
        Guid id,
        Request request,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) // getting userId from jwt
            ?? user.FindFirst("sub");

        if(userIdClaim == null)
            return Results.Unauthorized();

        var userId = Guid.Parse(userIdClaim.Value);

        //finding note
        var note = await db.Notes
            .FirstOrDefaultAsync(n => n.Id == id && n.userId == userId, ct);

        if(note == null)
            return Results.NotFound();

        //updating note
        note.Title = request.Title;
        note.Content = request.Content;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        Log.Information("Note with id {NoteId} updated by user {UserId}", userId, note.Id);

        return Results.NoContent();
    }
}
