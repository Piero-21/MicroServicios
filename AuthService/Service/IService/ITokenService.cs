using AuthService.Models;

namespace AuthService.Service.IService
{
    public interface ITokenService
    {
        public string GenerateToken(string userId, string email, string role);
        public Task<string?> GenerateRefreshToken(ApplicationUser user);
        public Task<string?> RefreshToken(string refreshToken);
    }
}
