namespace NoteTaking.Api.Common.models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
