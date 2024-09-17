using AspNetCoreHero.ToastNotification.Abstractions;
using AspNetCoreHero.ToastNotification.Notyf;
using ManejoClientes.Data;
using ManejoClientes.Models;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManejoEmpleados.Controllers
{
    public class PosicionesController : Controller
    {
        private readonly AppDbcontext appDbcontext;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly INotyfService notyfService;

        public PosicionesController(AppDbcontext appDbcontext,
            IServicioUsuarios servicioUsuarios,INotyfService notyfService)
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


            List<Posicion> lista;


            /*AspUser Es NULL cuando Logeamos con Cuenta Local
           Si es NULL significa que no estamos autenticandonos con cuenta externa 
           si no con cuenta local*/
            if (AspUser == null)
            {
                 lista = await appDbcontext.Posiciones.Where(x => x.UsuarioId ==
                usuarioId).ToListAsync();

            }
            else
            {

             lista = await appDbcontext.Posiciones.Where(x => x.UsuarioId == AspUser.Id).ToListAsync();
            }


            return View(lista);

        }


        [HttpGet]
        public IActionResult Nuevo()
        {

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Nuevo(Posicion posicion)
        {



            if (!ModelState.IsValid)
            {
                return View(posicion);

            }
            //Creando Nuevo Id para el empleado nuevo
            posicion.Id = Guid.NewGuid().ToString();


            //obteniendo Id del Usario
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            //usuario*
            var usuario = await appDbcontext.Usuarios.FirstOrDefaultAsync(u
                => u.Id == usuarioId || u.AspNetUserId == usuarioId);

            if (usuario == null)
            {
                notyfService.Warning("No se encontro la posicion");

                return RedirectToAction("NoEncontrado", "Home");
            }


            //asignando Al usuarioId del posicion el Id obtenido de usuario
            posicion.UsuarioId = usuario.Id;

            var existeIdDuplicado = await appDbcontext.Posiciones.AnyAsync(c => c.Id == posicion.Id);
            if (existeIdDuplicado)
            {
                //creando Nuevo ID en caso de que ya exista uno
                posicion.Id = Guid.NewGuid().ToString();
            }




            var existe = await ExistePosicion(posicion.Nombre, posicion.UsuarioId);

            if (existe)
            {
                ModelState.AddModelError(nameof(posicion.Nombre), $"La posicion {posicion.Nombre} ya existe");
                return View(posicion);
            }








            await appDbcontext.Posiciones.AddAsync(posicion);





            await appDbcontext.SaveChangesAsync();


            notyfService.Custom("Posicion agregada exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");




            return RedirectToAction("Lista", "Posiciones");

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
                    notyfService.Warning("No se encontro la posicion");

                    return RedirectToAction("NoEncontrado", "Home");

                }

                return View(departamento);
            }



            var PosicionExterna = await ObtenerPorId(id, AspUser.Id);


            if (PosicionExterna == null)
            {
                notyfService.Warning("No se encontro la posicion");

                return RedirectToAction("NoEncontrado", "Home");

            }

            return View(PosicionExterna);


        }


        [HttpPost]

        public async Task<IActionResult> Editar(Posicion posicion)
        {



            if (!ModelState.IsValid)
            {
                notyfService.Error("Ocurrio un error inesperado!");

                return View(posicion);

            }
            //Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);


            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {



                var ExistePosicioLocal = await ObtenerPorId(posicion.Id, usuarioId);

                if (ExistePosicioLocal == null)
                {
                    notyfService.Warning("No se encontro la posicion");

                    return RedirectToAction("NoEncontrado", "Home");
                }



                ExistePosicioLocal.Nombre = posicion.Nombre;



                await appDbcontext.SaveChangesAsync();

                notyfService.Custom("Posicion editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");


                return RedirectToAction("Lista", "Posiciones");
            }

            var ExistePosicionExterna = await ObtenerPorId(posicion.Id, AspUser.Id);


            if (ExistePosicionExterna == null)
            {
                notyfService.Warning("No se encontro la posicion");

                return RedirectToAction("NoEncontrado", "Home");

            }




            ExistePosicionExterna.Nombre = posicion.Nombre;

            await appDbcontext.SaveChangesAsync();
            
            notyfService.Custom("Posicion editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");


            return RedirectToAction("Lista", "Posiciones");

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
                var PosicionExiste = await ObtenerPorId(id, usuarioId);

                if (PosicionExiste == null)
                {
                    notyfService.Warning("No se encontro la posicion");

                    return RedirectToAction("NoEncontrado", "Home");

                }


                appDbcontext.Posiciones.Remove(PosicionExiste);



                await appDbcontext.SaveChangesAsync();
                notyfService.Custom("Posicion borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-pen");


                return RedirectToAction("Lista", "Posiciones");
            }

            //Si no es Null estamos autenticados con cuenta externa

            var PosicionExisteExterna = await ObtenerPorId(id, AspUser.Id);

            if (PosicionExisteExterna == null)
            {
                notyfService.Warning("No se encontro la posicion");

                return RedirectToAction("NoEncontrado", "Home");

            }




            appDbcontext.Posiciones.Remove(PosicionExisteExterna);




            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Posicion borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-pen");


            return RedirectToAction("Lista", "Posiciones");



        }



        //Obteniendo La posicion por su Id y el UsuarioId
        public async Task<Posicion> ObtenerPorId(string id, string usuarioId)
        {
            return await appDbcontext.Posiciones
                .FirstOrDefaultAsync(tc => tc.Id == id && tc.UsuarioId == usuarioId);
        }

        //Verificando Si ya existe la posicion a crear antes de postear la informacion
        //esto es para en el modelo remote

        [HttpGet]
        public async Task<IActionResult> Verificar(Posicion posicion)
        {
            posicion.UsuarioId = servicioUsuarios.ObtenerUsarioId();



            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == posicion.UsuarioId);



            //Si es NULL significa que es cuentaLocal
            if (AspUser == null)
            {
                var existe = await ExistePosicion(posicion.Nombre, posicion.UsuarioId);

                if (existe)
                {
                    return Json($"La {posicion.Nombre} ya existe!");

                }
                return Json(true);
            }

            //No es null significa que es CuentaExterna

            var existeExterno = await ExistePosicion(posicion.Nombre, posicion.UsuarioId);

            if (existeExterno)
            {
                return Json($"La posicion {posicion.Nombre} ya existe!");

            }
            return Json(true);

        }



        private async Task<bool> ExistePosicion(string NombrePosicion, string usuarioId)
        {
            return await appDbcontext.Posiciones.AnyAsync(c => c.Nombre == NombrePosicion &&
            c.UsuarioId == usuarioId);
        }
    }
}
