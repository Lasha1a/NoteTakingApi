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
        List<string> TagNames
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
            UserId = userId,
            Title = request.Title,
            Content = request.Content,
            IsDeleted = "false",
            CreatedAt = DateTime.UtcNow
        };

        if(request.TagNames is not null && request.TagNames.Any())
        {
            foreach(var tagName in request.TagNames.Distinct())
            {
                var normilized = tagName.Trim().ToLower();

                //finding existing tag
                var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == normilized, ct);

                //creating tag if its missing
                if(tag is null)
                {
                    tag = new Tag
                    {
                        Id = Guid.NewGuid(),
                        Name = normilized
                    };

                    db.Tags.Add(tag);
                }

                //junction row
                note.NoteTags.Add(new NoteTag
                {
                    NoteId = note.Id,
                    TagId = tag.Id,
                    Tag = tag
                });
            }
        }

        db.Notes.Add(note);
        await db.SaveChangesAsync(ct); // save the new note to the database

        Log.Information(
            "User {UserId} created note {NoteId} with tags {@Tags}",
            userId,
            note.Id,
            request.TagNames
        );

        return Results.Ok(new Response(note.Id));
    }
}
