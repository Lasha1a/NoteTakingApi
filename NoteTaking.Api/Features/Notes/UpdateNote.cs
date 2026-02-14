namespace NoteTaking.Api.Features.Notes;

public class UpdateNote
{
    public record class Request(
        string Title,
        string? Content
    );

    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPut("/notes/{id;Guid}", Handle)
}
