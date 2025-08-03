using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Dtos
{
    public class RegisterUserDto
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Apellido { get; set; }
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Correo { get; set; }
    }
}
