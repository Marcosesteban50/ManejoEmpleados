using ManejoClientes.Models;
using ManejoClientes.Validaciones;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ManejoEmpleados.Models
{
    public class Posicion
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El campo {0} Es Requerido!")]
        [PrimeraLetraMayus]
        [Remote(action: "Verificar", controller: "Posiciones")]

        public string Nombre { get; set; }

        public string UsuarioId { get; set; }

        public Usuario Usuario { get; set; }
    }
}
