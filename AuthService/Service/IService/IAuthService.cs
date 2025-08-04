namespace AuthService.Service.IService
{
    public interface IAuthService
    {
        Task<(bool Success, string? Token, string? RefreshToken, string? ErrorMessage)> RegisterAsync(string email, string password, string nombreDeUsuario, string rol = "User");
        public Task<(string? Token, string? RefreshToken)> LoginAsync(string email, string password);
        public Task<(string accessToken, string refreshToken)> RefreshToken(string refreshToken);
        public Task<bool> Logout(string id);
    }
}
