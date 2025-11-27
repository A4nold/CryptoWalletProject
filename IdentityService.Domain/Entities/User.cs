namespace IdentityService.Domain.Entities
{
    public class User
    {
        public Guid id { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<RefreshToken> RefreshToken { get; set; } = new();
    }
}
