using ManejoClientes.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ManejoEmpleados.Models
{
    public class EmpleadosCreacionViewModel : Empleado
    {
        public string DepartamentoNombre { get; set; }
        public string PosicionNombre { get; set; }
        public IEnumerable<SelectListItem> Departamentos { get; set; }
        public IEnumerable<SelectListItem> Posiciones { get; set; }
    }
}
