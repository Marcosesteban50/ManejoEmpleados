using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Office2013.Excel;
using ManejoClientes.Data;
using ManejoClientes.Models;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoEmpleados.Controllers
{
    public class DepartamentosController : Controller
    {
        private readonly AppDbcontext appDbcontext;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly INotyfService notyfService;

        public DepartamentosController(AppDbcontext appDbcontext, IServicioUsuarios servicioUsuarios, INotyfService notyfService)
        {
            this.appDbcontext = appDbcontext;
            this.servicioUsuarios = servicioUsuarios;
            this.notyfService = notyfService;
        }



        [HttpGet]

        public async Task<IActionResult> Lista()
        {

            //Obteniendo Id de cuentas local
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);

            List<Departamentos> lista;

            /*AspUser Es NULL cuando Logeamos con Cuenta Local
            Si es NULL significa que no estamos autenticandonos con cuenta externa 
            si no con cuenta local*/
            if (AspUser == null)
            {
                lista = await appDbcontext.Departamentos.Where(x => x.UsuarioId ==
               usuarioId).ToListAsync();

                return View(lista);
            }
            else
            {

                lista = await appDbcontext.Departamentos.Where(x => x.UsuarioId == AspUser.Id).ToListAsync();
            }

            return View(lista);
        }





        [HttpGet]
        public IActionResult Nuevo()
        {

            if (!ModelState.IsValid)
            {
                notyfService.Error("Ocurrio un error inesperado");
                
                return View("NoEncontrado", "Home");
            }


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Nuevo(Departamentos departamento)
        {



            if (!ModelState.IsValid)
            {
                notyfService.Error("Ocurrio un error inesperado");

                return View(departamento);

            }
            //Creando Nuevo Id para el empleado nuevo
            departamento.Id = Guid.NewGuid().ToString();


            //obteniendo Id del Uusario
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            //usuario*
            var usuario = await appDbcontext.Usuarios.FirstOrDefaultAsync(u
                => u.Id == usuarioId || u.AspNetUserId == usuarioId);

            if (usuario == null)
            {
                notyfService.Error("Ocurrio un error inesperado");

                return RedirectToAction("NoEncontrado", "Home");
            }


            //asignando Al usuarioId del departamento el Id obtenido de usuario
            departamento.UsuarioId = usuario.Id;

            var existeIdDuplicado = await appDbcontext.Empleados.AnyAsync(c => c.Id == departamento.Id);
            if (existeIdDuplicado)
            {
                //creando Nuevo ID en caso de que ya exista uno
                departamento.Id = Guid.NewGuid().ToString();
            }




            var existe = await ExisteDepartamento(departamento.Nombre, departamento.UsuarioId);

            if (existe)
            {
                ModelState.AddModelError(nameof(departamento.Nombre), $"El departamento {departamento.Nombre} ya existe");
                return View(departamento);
            }








            await appDbcontext.Departamentos.AddAsync(departamento);





            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("departamento agregado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-pen");


            return RedirectToAction("Lista", "Departamentos");

        }




        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            //Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);

            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {
                var departamento = await ObtenerPorId(id, usuarioId);


                if (departamento == null)
                {
                    notyfService.Warning("No se encontro el departamento");

                    return RedirectToAction("NoEncontrado", "Home");

                }

                return View(departamento);
            }



            var DepartamentoExterno = await ObtenerPorId(id, AspUser.Id);


            if (DepartamentoExterno == null)
            {
                notyfService.Warning("No se encontro el departamento");

                return RedirectToAction("NoEncontrado", "Home");

            }

            return View(DepartamentoExterno);


        }


        [HttpPost]

        public async Task<IActionResult> Editar(Departamentos departamento)
        {



            if (!ModelState.IsValid)
            {
                notyfService.Error("Ocurrio un error inesperado!");

                return View(departamento);

            }
            //Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);


            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {



                var ExisteDepartamentoLocal = await ObtenerPorId(departamento.Id, usuarioId);

                if (ExisteDepartamentoLocal == null)
                {
                    notyfService.Warning("No se encontro el departamento");

                    return RedirectToAction("NoEncontrado", "Home");
                }



                ExisteDepartamentoLocal.Nombre = departamento.Nombre;



                await appDbcontext.SaveChangesAsync();

                notyfService.Custom("Departamente editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");

                return RedirectToAction("Lista", "Departamentos");
            }

            var ExisteDepartamentoExterno = await ObtenerPorId(departamento.Id, AspUser.Id);


            if (ExisteDepartamentoExterno == null)
            {
                notyfService.Warning("No se encontro el departamento");

                return RedirectToAction("NoEncontrado", "Home");

            }




            ExisteDepartamentoExterno.Nombre = departamento.Nombre;

            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Departamento editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");



            return RedirectToAction("Lista", "Departamentos");

        }






        [HttpGet]
        public async Task<IActionResult> Borrar(string id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);


            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {
                var DepartamentoExiste = await ObtenerPorId(id, usuarioId);

                if (DepartamentoExiste == null)
                {
                    notyfService.Warning("No se encontro el departamento");

                    return RedirectToAction("NoEncontrado", "Home");

                }


                appDbcontext.Departamentos.Remove(DepartamentoExiste);



                await appDbcontext.SaveChangesAsync();
                notyfService.Custom("Departamento borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-pen");


                return RedirectToAction("Lista", "Departamentos");
            }

            //Si no es Null estamos autenticados con cuenta externa

            var DepartamentoExisteExterno = await ObtenerPorId(id, AspUser.Id);

            if (DepartamentoExisteExterno == null)
            {
                notyfService.Warning("No se encontro el departamento");

                return RedirectToAction("NoEncontrado", "Home");

            }




            appDbcontext.Departamentos.Remove(DepartamentoExisteExterno);




            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Departamento borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-pen");


            return RedirectToAction("Lista", "Departamentos");



        }




        //Obteniendo El Departamento por su Id y el UsuarioId
        public async Task<Departamentos> ObtenerPorId(string id, string usuarioId)
        {
            return await appDbcontext.Departamentos
                .FirstOrDefaultAsync(tc => tc.Id == id && tc.UsuarioId == usuarioId);
        }

        //Verificando Si ya existe el departamento a crear antes de postear la informacion
        //esto es para en el modelo remote

        [HttpGet]
        public async Task<IActionResult> Verificar(Departamentos departamento)
        {
            departamento.UsuarioId = servicioUsuarios.ObtenerUsarioId();



            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == departamento.UsuarioId);



            //Si es NULL significa que es cuentaLocal
            if (AspUser == null)
            {
                var existe = await ExisteDepartamento(departamento.Nombre, departamento.UsuarioId);

                if (existe)
                {
                    return Json($"El departamento {departamento.Nombre} ya existe!");

                }
                return Json(true);
            }

            //No es null significa que es CuentaExterna

            var existeExterno = await ExisteDepartamento(departamento.Nombre, departamento.UsuarioId);

            if (existeExterno)
            {
                return Json($"El departamento {departamento.Nombre} ya existe!");

            }
            return Json(true);

        }



        private async Task<bool> ExisteDepartamento(string NombreDepartamento, string usuarioId)
        {
            return await appDbcontext.Departamentos.AnyAsync(c => c.Nombre == NombreDepartamento &&
            c.UsuarioId == usuarioId);
        }


    }
}