namespace NoteTaking.Api.Common.models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;

        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
