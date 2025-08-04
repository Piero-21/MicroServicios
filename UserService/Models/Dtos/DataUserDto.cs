using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Dtos
{
    public class DataUserDto
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Nombre { get; set; } 
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Apellido { get; set; } 
        public string? Email { get; set; }
    }
}
