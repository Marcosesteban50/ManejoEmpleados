using ManejoClientes.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoClientes.Models
{
    public class RegistroViewModel
    {

        [Required(ErrorMessage = "El campo {0} Es Requerido!")]
        [EmailAddress(ErrorMessage = "Correo Electronico No Valido")]
        
       
        public string Email { get; set; }
        [Required(ErrorMessage = "El campo {0} Es Requerido!")]

        public string Password { get; set; }
    }
}
