using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Common.models;
using NoteTaking.Api.Infrastructure.Data;
using System.Security.Claims;
using Serilog;

namespace NoteTaking.Api.Features.Notes;

public static class CreateNote
{

    // record types for request and response
    public record Request (
        string Title,
        string? Content,
        List<Guid>? TagIds
    );

    public record Response (
        Guid Id
    );

    // endpoint mapping
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/notes", Handle)
            .RequireAuthorization()
            .WithName(nameof(CreateNote))
            .WithOpenApi();

    // handler method
    static async Task<IResult> Handle(
        Request request,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) // try to find the claim with type "nameidentifier" first
            ?? user.FindFirst("sub");

        if(userIdClaim is null)
        {
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var note = new Note // create a new note 
        {
            Id = Guid.NewGuid(),
            userId = userId,
            Title = request.Title,
            Content = request.Content,
            IsDeleted = "false",
            CreatedAt = DateTime.UtcNow
        };

        if(request.TagIds is not null && request.TagIds.Any())
        {
            var tags = await db.Tags // get the tags from the database that match the provided tag IDs
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(ct);

            foreach (var tagId in tags) // for each tag ID, create a new NoteTag entry 
            {
                note.NoteTags.Add(new NoteTag
                {
                    NoteId = note.Id,
                    TagId = tagId
                });
            }
        }

        db.Notes.Add(note);
        await db.SaveChangesAsync(ct); // save the new note to the database

        Log.Information(
            "User {UserId} created a new note with ID {NoteId}",
            userId,
            note.Id
        );

        return Results.Ok(new Response(note.Id));
    }
}
