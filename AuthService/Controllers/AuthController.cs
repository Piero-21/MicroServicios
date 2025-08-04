using AuthService.Models.Dtos;
using AuthService.Service.IService;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TallerApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseApi _responseApi;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
            _responseApi = new();
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto.Email, dto.Password, dto.Username, dto.Role);
            if (!result.Success)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _responseApi.ErrorMessages.Add(result.ErrorMessage ?? "Error al registrar el usuario");
                return BadRequest(_responseApi);
            }
            _responseApi.IsSuccess = true;
            _responseApi.StatusCode = System.Net.HttpStatusCode.Created;
            _responseApi.Result = new { Token = result.Token };
            var refreshCookieOptions = new CookieOptions
            {
                Domain = "localhost",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);
            return CreatedAtAction(nameof(Register), _responseApi);
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto.Email, dto.Password);
            if (result.Token == null)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                _responseApi.ErrorMessages.Add("Credenciales inválidas");
                return Unauthorized(_responseApi);
            }
            _responseApi.IsSuccess = true;
            _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
            _responseApi.Result = new { Token = result.Token };
            var refreshCookieOptions = new CookieOptions
            {
                Domain = "localhost",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);
            return Ok(_responseApi);
        }

        [HttpGet("Me")]
        public IActionResult Me()
        {
            _responseApi.IsSuccess = false;
            _responseApi.StatusCode = System.Net.HttpStatusCode.Unauthorized;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) _responseApi.ErrorMessages.Add("No se ha encontrado usuario.");
            else
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : Unauthorized(_responseApi);
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh()
        {
            var _responseApi = new ResponseApi
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                IsSuccess = false
            };
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _responseApi.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                _responseApi.ErrorMessages.Add("Invalid refresh token.");
                return Unauthorized(_responseApi);
            }
            var (newAccessToken, newRefreshToken) = await _authService.RefreshToken(refreshToken);
            var refreshCookieOptions = new CookieOptions
            {
                Domain = "localhost",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refresh_token", newRefreshToken, refreshCookieOptions);
            _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
            _responseApi.Result = new { token = newAccessToken };
            _responseApi.IsSuccess = true;
            return Ok(_responseApi);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            _responseApi.IsSuccess = false;
            _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
            var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(id))
            {
                _responseApi.ErrorMessages.Add("No se ha encontrado al usuario.");
                return BadRequest(_responseApi);
            }
            if (await _authService.Logout(id))
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
                Response.Cookies.Delete("refresh_token");
            }
            else
            {
                _responseApi.ErrorMessages.Add("No se ha encontrado una sesión activa.");
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : BadRequest(_responseApi);
        }
    }
}
