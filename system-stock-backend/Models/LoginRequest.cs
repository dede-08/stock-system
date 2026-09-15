using System.ComponentModel.DataAnnotations;

namespace api_gestion_productos.Models;

public class LoginRequest
{
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    public string email { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string password { get; set; } = "";
}
