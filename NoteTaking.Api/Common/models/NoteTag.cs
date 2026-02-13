namespace NoteTaking.Api.Common.models
{
    public class NoteTag
    {
        public Guid NoteId { get; set; }
        public Note Note { get; set; } = default!;

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = default!;
    }
}
