using ManejoClientes.Validaciones;
using ManejoEmpleados.Models;
using System.ComponentModel.DataAnnotations;

namespace ManejoClientes.Models
{
    public class Usuario
    {

        public string Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido!")]
        [EmailAddress(ErrorMessage = "Correo electronico no valido")]
      
        public string Email { get; set; }
        public string EmailNormalizado { get; set; }
        public string PasswordHash { get; set; }
        
        public string AspNetUserId { get; set; }

        public int EmpleadosContratados { get; set; } 
        public int EmpleadosDespedidos { get; set; }

        public decimal SalarioEmpleados { get; set; }

       

    }
}
