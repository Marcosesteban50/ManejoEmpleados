using ManejoClientes.Validaciones;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManejoClientes.Models
{
    public class Empleado
    {

       
        public string Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido!")]
        [PrimeraLetraMayus]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido!")]
        [EmailAddress(ErrorMessage = "Correo electronico no valido")]
        
        [Remote(action: "Verificar", controller:"Empleados")]

        public string Email { get; set; }
        public string UsuarioId { get; set; }
        
        public Usuario Usuario { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido!")]

        //valor entre 1 y valor maximo de un decimal
        [Range(1, (double)decimal.MaxValue, ErrorMessage = "El valor debe ser mayor que 0.")]
        public decimal Salario { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido!")]

        public string Telefono {  get; set; }

      

        [Required(ErrorMessage = "El campo {0} es requerido!")]

        public DateTime FechaContrato { get; set; }

        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El campo departamento es requerido!")]

        public string DepartamentoId { get; set; }
        public Departamentos Departamento { get; set; }
        
        [Required(ErrorMessage = "El campo posiciones es requerido!")]
        public string PosicionId { get; set; }



        public Posicion Posicion { get; set; }

        public bool Activo { get; set; }



    }
}
