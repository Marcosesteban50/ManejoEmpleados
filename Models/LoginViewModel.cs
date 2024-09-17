using ManejoClientes.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace ManejoClientes.Models
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "El campo {0} Es Requerido!")]
        [EmailAddress(ErrorMessage = "Correo Electronico No Valido")]
        

        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} Es Requerido!")]

        public string Password { get; set; }

        public bool Recuerdame { get; set; }
    }
}
