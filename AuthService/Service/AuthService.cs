using AuthService.Models;
using AuthService.Service.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Service
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
        }
        public async Task<(string? Token, string? RefreshToken)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return (null, null);

            if (!await _userManager.CheckPasswordAsync(user, password))
                return (null, null);

            var roles = await _userManager.GetRolesAsync(user);
            return (_tokenService.GenerateToken(user.Id, user.Email!, roles.FirstOrDefault() ?? "User"), await _tokenService.GenerateRefreshToken(user));
        }

        public async Task<(bool Success, string? Token, string? RefreshToken, string? ErrorMessage)> RegisterAsync(string email, string password, string nombreUsuario, string rol = "User")
        {
            var user = new ApplicationUser
            {
                UserName = nombreUsuario,
                NormalizedUserName = nombreUsuario.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, null, null, errorMessage);
            }
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("User"));
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, rol);
            }

            var token = _tokenService.GenerateToken(user.Id, user.Email!, rol);
            var refreshToken = await _tokenService.GenerateRefreshToken(user);
            return (true, token, refreshToken, null);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshToken(string refreshToken)
        {
            var users = await _userManager.Users.ToListAsync();
            var user = users.FirstOrDefault(u =>
                                _userManager.GetAuthenticationTokenAsync(u, "ApiTickets", "RefreshToken").Result == refreshToken);
            if (user == null)
            {
                return (string.Empty, string.Empty);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var newAccessToken = _tokenService.GenerateToken(user.Id, user.Email, roles[0]); //Utils.GenerateJwtToken(user, roles, secretKey);
            var newRefreshToken = Guid.NewGuid().ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "ApiTickets", "RefreshToken", newRefreshToken);
            return (newAccessToken, newRefreshToken);
        }

        public async Task<bool> Logout(string id)
        {
            bool bReturn = false;
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (!string.IsNullOrEmpty(await _userManager.GetAuthenticationTokenAsync(user, "ApiTickets", "RefreshToken")))
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, "ApiTickets", "RefreshToken");
                bReturn = true;
            }
            return bReturn;
        }
    }
}
