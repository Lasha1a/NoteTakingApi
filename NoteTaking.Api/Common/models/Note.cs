namespace NoteTaking.Api.Common.models
{
    public class Note
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public string IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User User { get; set; } = default!;
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
