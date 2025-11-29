namespace IdentityService.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid AppUserId { get; set; }
        public string Token { get; set; } = default!;

        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }

        // Convenience property
        public bool IsActive => RevokedAt == null && DateTimeOffset.UtcNow < ExpiresAt;

        public AppUser AppUser { get; set; } = default!;
    }
}
