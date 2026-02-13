using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Common.models;
using NoteTaking.Api.Infrastructure.Data;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public static class CreateNote
{
    public record Request (
        string Title,
        string? Content,
        List<Guid>? TagIds
    );

    public record Response (
        Guid Id
    );

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/notes", Handle)
            .RequireAuthorization()
            .WithName(nameof(CreateNote))
            .WithOpenApi();

    static async Task<IResult> Handle(
        Request request,
        AppDbContext db,
        ClaimsPrincipal user,
        CancellationToken ct)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub");

        if(userIdClaim is null)
        {
            return Results.Unauthorized();
        }

        var userId = Guid.Parse(userIdClaim.Value);

        var note = new Note
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
            var tags = await db.Tags
                .Where(t => request.TagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync(ct);

            foreach (var tagId in tags)
            {
                note.NoteTags.Add(new NoteTag
                {
                    NoteId = note.Id,
                    TagId = tagId
                });
            }
        }

        db.Notes.Add(note);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new Response(note.Id));
    }
}
