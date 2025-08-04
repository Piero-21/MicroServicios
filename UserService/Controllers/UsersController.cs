using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TallerApi.Models;
using UserService.Models;
using UserService.Models.Dtos;
using UserService.Repository.IRepository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        protected ResponseApi _responseApi;
        private readonly IMapper _mapper;

        public UsersController(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _responseApi = new();
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
                var users = await _repo.GetAllAsync();
                if (users == null || !users.Any())
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _responseApi.ErrorMessages.Add("No se encontraron usuarios");
                    return NotFound(_responseApi);
                }
                _responseApi.Result = _mapper.Map<List<DataUserDto>>(users);
            }
            catch (Exception ex)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al obtener los usuarios");
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : StatusCode(500, _responseApi);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
                var user = await _repo.GetAsync(id);
                if (user == null)
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _responseApi.ErrorMessages.Add("Usuario no encontrado");
                    return NotFound(_responseApi);
                }
                _responseApi.Result = _mapper.Map<DataUserDto>(user);
            }
            catch (Exception)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al obtener el usuario");
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : StatusCode(500, _responseApi);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterUserDto user)
        {
            string userId = string.Empty;
            try
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.Created;
                if (!ModelState.IsValid || user == null)
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Datos del usuario no válidos");
                    return BadRequest(_responseApi);
                }
                var newUser = _mapper.Map<User>(user);
                userId = User.Claims.FirstOrDefault(u => u.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                    _responseApi.ErrorMessages.Add("Usuario no autenticado");
                    return Unauthorized(_responseApi);
                }
                newUser.Id = userId;
                newUser.Deleted = false;
                if (!await _repo.AddAsync(newUser))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Error al agregar el usuario");
                    return BadRequest(_responseApi);
                }
                _responseApi.Result = _mapper.Map<DataUserDto>(newUser);
            }
            catch (Exception ex)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al agregar el usuario");
            }
            return _responseApi.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = userId }, _responseApi) : StatusCode(500, _responseApi);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, DataUserDto user)
        {
            try
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
                if (!ModelState.IsValid || user == null || id == string.Empty)
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    return BadRequest("Usuario no válido");
                }
                var existingUser = await _repo.GetAsync(id);
                if (existingUser == null)
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _responseApi.ErrorMessages.Add("Usuario no encontrado");
                    return NotFound(_responseApi);
                }
                if (!(/*Guid.TryParse(id, out var guidId) && */await _repo.UpdateAsync(id, user)))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Error al actualizar el usuario");
                    return BadRequest(_responseApi);
                }
                var updatedUser = await _repo.GetAsync(id);
                _responseApi.Result = updatedUser;
            }
            catch (Exception)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al actualizar el usuario");
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : StatusCode(500, _responseApi);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                _responseApi.IsSuccess = true;
                _responseApi.StatusCode = System.Net.HttpStatusCode.OK;
                if (string.IsNullOrEmpty(id))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Id de usuario no válido");
                    return BadRequest(_responseApi);
                }
                var existingUser = await _repo.GetAsync(id);
                if (existingUser == null)
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.NotFound;
                    _responseApi.ErrorMessages.Add("Usuario no encontrado");
                    return NotFound(_responseApi);
                }
                if (!await _repo.DeleteAsync(id))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Error al eliminar el usuario");
                    return BadRequest(_responseApi);
                }
            }
            catch (Exception)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al eliminar el usuario");
            }
            return _responseApi.IsSuccess ? Ok(_responseApi) : StatusCode(500, _responseApi);
        }
    }
}
