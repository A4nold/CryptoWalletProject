namespace IdentityService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;

        public bool EmailConfirmed { get; set; }

        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
