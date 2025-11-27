namespace IdentityService.Domain.Entities
{
    public class RefreshToken
    {
        public Guid id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = default!;
    }
}
