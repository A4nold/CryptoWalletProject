using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities
{
    public class AppUser: IdentityUser<Guid>
    {
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Navigation
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}
