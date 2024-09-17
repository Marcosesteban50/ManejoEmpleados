using ManejoClientes.Data;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoClientes.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbcontext appDbcontext;
        private readonly IServicioUsuarios servicioUsuarios;

        public DashboardController(AppDbcontext appDbcontext,
            IServicioUsuarios servicioUsuarios)
        {
            this.appDbcontext = appDbcontext;
            this.servicioUsuarios = servicioUsuarios;
        }
        public async Task<IActionResult> DashboardGeneral()
        {
            //Id Cuenta local
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);


            
            //Si es null estamos con cuenta local
            if (AspUser == null)
            {
                var User = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
                u.Id == usuarioId);

               


                return View(User);

            }

            return View(AspUser);
        }





    }
}
