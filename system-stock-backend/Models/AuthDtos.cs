using System.ComponentModel.DataAnnotations;

namespace api_gestion_productos.Models;

// Registro: el cliente NUNCA envía id/isActive/role.
public class RegisterDto
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    public string name { get; set; } = "";

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
    public string lastname { get; set; } = "";

    [Required(ErrorMessage = "La edad es obligatoria")]
    [Range(18, 120, ErrorMessage = "La edad debe estar entre 18 y 120 años")]
    public int age { get; set; }

    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede tener más de 100 caracteres")]
    public string email { get; set; } = "";

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede tener más de 20 caracteres")]
    public string telephone { get; set; } = "";

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
    public string password { get; set; } = "";
}

public class AuthResponseDto
{
    public string token { get; set; } = "";
    public string email { get; set; } = "";
    public string fullName { get; set; } = "";
}
