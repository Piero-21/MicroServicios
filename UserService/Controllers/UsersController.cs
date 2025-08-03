using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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

        public UsersController(IUserRepository repo)
        {
            _repo = repo;
            _responseApi = new();
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
                _responseApi.Result = users;
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
                _responseApi.Result = user;
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
        public async Task<IActionResult> Post([FromBody] User user)
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
                if (!await _repo.AddAsync(user))
                {
                    _responseApi.IsSuccess = false;
                    _responseApi.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _responseApi.ErrorMessages.Add("Error al agregar el usuario");
                    return BadRequest(_responseApi);
                }
                _responseApi.Result = "Usuario agregado correctamente";
            }
            catch (Exception ex)
            {
                _responseApi.IsSuccess = false;
                _responseApi.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _responseApi.ErrorMessages.Add("Error al agregar el usuario");
            }
            return _responseApi.IsSuccess ? CreatedAtAction(nameof(GetById), userId,_responseApi) : StatusCode(500, _responseApi);
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
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID de usuario no válido");
            }
            var user = await _repo.GetAsync(id);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            return await _repo.DeleteAsync(id) ? Ok("Usuario eliminado") : BadRequest("Error al eliminar el usuario");
        }
    }
}
