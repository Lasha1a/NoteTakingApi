using Microsoft.EntityFrameworkCore;
using NoteTaking.Api.Infrastructure.Data;
using System.Security.Claims;

namespace NoteTaking.Api.Features.Notes;

public class FilterNotesByTags
{
    public record Response(
        Guid Id,
        string Title,
        string? Content,
        List<string> Tags
    );

    public static void Map(IEndpointRouteBuilder App) =>
        App.MapGet("/notes/filterByTags", Handle)
            .RequireAuthorization()
            .WithName(nameof(FilterNotesByTags))
            .WithOpenApi();

    static async Task<IResult> Handle(
        string tags,
        ClaimsPrincipal user,
        AppDbContext db,
        CancellationToken ct)
    {
        var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!); // get user ID from claims

        var tagNames = tags //split tags from queries
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();

        if (tagNames.Count == 0)
            return Results.BadRequest("At least one tag is required");


        var notes = await db.Notes // query notes with the given tags, userId and not deleted
            .Include(n => n.NoteTags)
                .ThenInclude(nt => nt.Tag)
            .Where(n =>
                 n.userId == userId &&
                 n.IsDeleted == "false" &&
                 n.NoteTags.Any(nt => tagNames.Contains(nt.Tag.Name))
            )
            .ToListAsync(ct);

        var result = notes.Select(n => new Response( // map notes to response
            n.Id,
            n.Title,
            n.Content,
            n.NoteTags.Select(nt => nt.Tag.Name).ToList()
        ));

        return Results.Ok(result);
    }


}
