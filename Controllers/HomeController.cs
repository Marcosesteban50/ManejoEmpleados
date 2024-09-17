using ManejoClientes.Data;
using ManejoClientes.Models;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace ManejoClientes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbcontext appDbcontext;
        private readonly IServicioUsuarios servicioUsuarios;

        public HomeController(ILogger<HomeController> logger,
            AppDbcontext appDbcontext, IServicioUsuarios servicioUsuarios)
        {
            _logger = logger;
            this.appDbcontext = appDbcontext;
            this.servicioUsuarios = servicioUsuarios;
        }





        public ActionResult ErrorSalario()
        {
            return View();
        }




        public async Task<IActionResult> Index()
        {


            var usuarioId = servicioUsuarios.ObtenerUsarioId();
            var AspUser = await appDbcontext.Usuarios
                .FirstOrDefaultAsync(u =>
                u.AspNetUserId == usuarioId);



            //Estamos en cuenta local 
            if (AspUser == null)
            {


                var datos = appDbcontext.Usuarios.Where(
                    u => u.Id == usuarioId).Include(x => x.SalarioEmpleados);

                


                return View(datos);


            }


            var datosExterno = appDbcontext.Usuarios.Where(
                u => u.AspNetUserId == usuarioId).Include(x => x.SalarioEmpleados);

            return View(datosExterno);
        }




        public IActionResult NoEncontrado()
        {
            return View();
        }

        public IActionResult NoAutenticado()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}