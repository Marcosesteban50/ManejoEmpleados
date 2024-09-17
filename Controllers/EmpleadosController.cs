using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Office2010.Excel;
using ManejoClientes.Data;
using ManejoClientes.Models;
using ManejoEmpleados.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using System.Collections;
using System.Security.Cryptography;

namespace ManejoClientes.Controllers
{

    public class EmpleadosController : Controller
    {
        private readonly AppDbcontext appDbcontext;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly INotyfService notyfService;

        public EmpleadosController(AppDbcontext appDbcontext,
            IServicioUsuarios servicioUsuarios,
            INotyfService notyfService)
        {
            this.appDbcontext = appDbcontext;
            this.servicioUsuarios = servicioUsuarios;
            this.notyfService = notyfService;
        }










        [HttpGet]

        public async Task<IActionResult> Lista()
        {
            var usuarioId = servicioUsuarios.ObtenerUsarioId();
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);

            List<Empleado> lista;

            if (AspUser == null)
            {
                lista = await appDbcontext.Empleados
                    .Where(x => x.UsuarioId == usuarioId).Include(x => x.Posicion).Include(x => x.Departamento)

                    .ToListAsync();
            }
            else
            {
                lista = await appDbcontext.Empleados
                    .Where(x => x.UsuarioId == AspUser.Id).Include(x => x.Posicion).Include(x => x.Departamento)

                    .ToListAsync();
            }

            var empleadosViewModel = lista.Select(c => new EmpleadosCreacionViewModel
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Email = c.Email,
                Activo = c.Activo,
                Telefono = c.Telefono,
                FechaContrato = c.FechaContrato,
                Salario = c.Salario,
                DepartamentoNombre = c.Departamento.Nombre,
                PosicionNombre = c.Posicion.Nombre


            }).ToList();

            return View(empleadosViewModel);
        }














        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {









            //usuarioId
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            var modelo1 = new EmpleadosCreacionViewModel
            {
                FechaContrato = DateTime.Today,
                Departamentos = await ObtenerDepartamentos(usuarioId),
                Posiciones = await ObtenerPosiciones(usuarioId)
            };

            return View(modelo1);

        }


        [HttpPost]
        public async Task<IActionResult> Nuevo(EmpleadosCreacionViewModel empleado)
        {

            // obteniendo Id del Usario
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            if (!ModelState.IsValid)
            {
                empleado.Posiciones = await ObtenerPosiciones(usuarioId);

                empleado.Departamentos = await ObtenerDepartamentos(usuarioId);
                notyfService.Error("Ocurrio un error inesperado");
                return View(empleado);


            }


            //Creando Nuevo Id para el empleado nuevo
            empleado.Id = Guid.NewGuid().ToString();




            //usuario*
            var usuario = await appDbcontext.Usuarios.FirstOrDefaultAsync(u
                => u.Id == usuarioId || u.AspNetUserId == usuarioId);

            if (usuario == null)
            {
                notyfService.Warning("Cuenta no encontrada!");

                return RedirectToAction("NoEncontrado", "Home");
            }


            //Asignando al usuarioId del empleado el Id obtenido de usuario
            empleado.UsuarioId = usuario.Id;

            var existeIdDuplicado = await appDbcontext.Empleados.AnyAsync(c => c.Id == empleado.Id);
            if (existeIdDuplicado)
            {
                //creando Nuevo ID en caso de que ya exista uno
                empleado.Id = Guid.NewGuid().ToString();
            }




            var existe = await ExisteCorreo(empleado.Email, empleado.UsuarioId);

            if (existe)
            {
                empleado.Posiciones = await ObtenerPosiciones(usuarioId);

                empleado.Departamentos = await ObtenerDepartamentos(usuarioId);
                ModelState.AddModelError(nameof(empleado.Email), $"El correo {empleado.Email} ya existe");

                return View(empleado);
            }





            usuario.SalarioEmpleados += empleado.Salario;


            await appDbcontext.Empleados.AddAsync(empleado);
            usuario.EmpleadosContratados++;







            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Empleado creado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-plus");


            return RedirectToAction("Lista", "Empleados");

        }










        [HttpGet]
        public async Task<IActionResult> Editar(string id)
        {
            // Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            // Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);





            Empleado empleado;


            // Si AspUser es null significa que estamos logeados con cuenta local y no externa
            if (AspUser == null)
            {
                empleado = await ObtenerPorId(id, usuarioId);

                if (empleado == null)
                {
                    notyfService.Warning("Empleado no encontrado!");

                    return RedirectToAction("NoEncontrado", "Home");
                }
            }
            else
            {
                empleado = await ObtenerPorId(id, AspUser.Id);

                if (empleado == null)
                {
                    notyfService.Warning("Empleado no encontrado!");

                    return RedirectToAction("NoEncontrado", "Home");
                }
            }

            // Crear el modelo para la vista, incluyendo el salario anual calculado
            var modelo = new EmpleadosCreacionViewModel
            {
                Id = empleado.Id,
                Nombre = empleado.Nombre,
                Email = empleado.Email,
                Activo = empleado.Activo,
                Telefono = empleado.Telefono,
                FechaContrato = empleado.FechaContrato,
                Salario = empleado.Salario,
                DepartamentoId = empleado.DepartamentoId,
                PosicionId = empleado.PosicionId,
                Departamentos = await ObtenerDepartamentos(usuarioId),
                Posiciones = await ObtenerPosiciones(usuarioId)
            };

            // Guardar el salario antiguo en la vista usando ViewBag
            ViewBag.salarioAntiguo = empleado.Salario;


            return View(modelo);
        }



        [HttpPost]

        //El decimal salarioAntiguo es para el viewBag
        public async Task<IActionResult> Editar(EmpleadosCreacionViewModel empleado, decimal salarioAntiguo)
        {

            //Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();





            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);



            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {


                var usuario = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
                    u.Id == usuarioId);

                if (usuario == null)
                {
                    notyfService.Warning("Usuario no encontrada!");
                    return RedirectToAction("NoEncontrado", "Home");
                }


                var ExisteEmpleadoEnCuentaLocal = await ObtenerPorId(empleado.Id, usuarioId);

                if (ExisteEmpleadoEnCuentaLocal == null)
                {
                    notyfService.Warning("Empleado no encontrado!");

                    return RedirectToAction("NoEncontrado", "Home");
                }



                ExisteEmpleadoEnCuentaLocal.Nombre = empleado.Nombre;
                ExisteEmpleadoEnCuentaLocal.Email = empleado.Email;
                ExisteEmpleadoEnCuentaLocal.Salario = empleado.Salario;
                ExisteEmpleadoEnCuentaLocal.Posicion = empleado.Posicion;
                ExisteEmpleadoEnCuentaLocal.Telefono = empleado.Telefono;
                ExisteEmpleadoEnCuentaLocal.FechaContrato = empleado.FechaContrato;
                ExisteEmpleadoEnCuentaLocal.Activo = empleado.Activo;
                ExisteEmpleadoEnCuentaLocal.DepartamentoId = empleado.DepartamentoId;
                ExisteEmpleadoEnCuentaLocal.PosicionId = empleado.PosicionId;





                if (salarioAntiguo > ExisteEmpleadoEnCuentaLocal.Salario)
                {
                    usuario.SalarioEmpleados += ExisteEmpleadoEnCuentaLocal.Salario;
                    usuario.SalarioEmpleados -= salarioAntiguo;
                }

                if (salarioAntiguo < ExisteEmpleadoEnCuentaLocal.Salario)
                {


                    usuario.SalarioEmpleados -= salarioAntiguo;
                    usuario.SalarioEmpleados += ExisteEmpleadoEnCuentaLocal.Salario;

                }

                if (salarioAntiguo == ExisteEmpleadoEnCuentaLocal.Salario)
                {
                    usuario.SalarioEmpleados = usuario.SalarioEmpleados;
                }





                await appDbcontext.SaveChangesAsync();

                notyfService.Custom("Empleado editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");


                return RedirectToAction("Lista", "Empleados");
            }


            //Cuenta externas
            //---------------------------------

            var usuario2 = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);


            if (usuario2 == null)
            {
                notyfService.Warning("Cuenta no encontrada");

                return RedirectToAction("NoEncontrado", "Home");
            }


            var ExisteEmpleadoEnCuentaExterna = await ObtenerPorId(empleado.Id, AspUser.Id);


            if (ExisteEmpleadoEnCuentaExterna == null)
            {
                notyfService.Warning("Empleado no encontrado!");

                return RedirectToAction("NoEncontrado", "Home");

            }




            ExisteEmpleadoEnCuentaExterna.Nombre = empleado.Nombre;
            ExisteEmpleadoEnCuentaExterna.Email = empleado.Email;
            ExisteEmpleadoEnCuentaExterna.Salario = empleado.Salario;
            ExisteEmpleadoEnCuentaExterna.Posicion = empleado.Posicion;
            ExisteEmpleadoEnCuentaExterna.Telefono = empleado.Telefono;
            ExisteEmpleadoEnCuentaExterna.FechaContrato = empleado.FechaContrato;
            ExisteEmpleadoEnCuentaExterna.Activo = empleado.Activo;
            ExisteEmpleadoEnCuentaExterna.DepartamentoId = empleado.DepartamentoId;
            ExisteEmpleadoEnCuentaExterna.PosicionId = empleado.PosicionId;


           



            if (salarioAntiguo > ExisteEmpleadoEnCuentaExterna.Salario)
            {
                usuario2.SalarioEmpleados += ExisteEmpleadoEnCuentaExterna.Salario;
                usuario2.SalarioEmpleados -= salarioAntiguo;
            }

            if (salarioAntiguo < ExisteEmpleadoEnCuentaExterna.Salario)
            {


                usuario2.SalarioEmpleados -= salarioAntiguo;
                usuario2.SalarioEmpleados += ExisteEmpleadoEnCuentaExterna.Salario;

            }

            if (salarioAntiguo == ExisteEmpleadoEnCuentaExterna.Salario)
            {
                usuario2.SalarioEmpleados = usuario2.SalarioEmpleados;
            }

            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Empleado editado exitosamente", 7000, "#8790ae", "fa-solid fa-user-pen");




            return RedirectToAction("Lista", "Empleados");

        }






        [HttpGet]
        public async Task<IActionResult> Borrar(string id)
        {
            //Obteniendo Id de CuentaLocal
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);

            //Si es Null significa que estamos logeados con cuenta local y no externa



            if (AspUser == null)
            {
                var ExisteEmpleadoEnCuentaLocal = await ObtenerPorId(id, usuarioId);


                if (ExisteEmpleadoEnCuentaLocal == null)
                {
                    notyfService.Warning("No se encontro el empleado");



                    return RedirectToAction("NoEncontrado", "Home");

                }



                return View(ExisteEmpleadoEnCuentaLocal);
            }


            var usuario2 = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);


            if (usuario2 == null)
            {
                notyfService.Warning("Cuenta no encontrada");

                return RedirectToAction("NoEncontrado", "Home");
            }


            var ExisteEmpleadoEnCuentaExterna = await ObtenerPorId(id, AspUser.Id);


            if (ExisteEmpleadoEnCuentaExterna == null)
            {
                notyfService.Warning("Empleado no encontrada");


                return RedirectToAction("NoEncontrado", "Home");

            }



            return View(ExisteEmpleadoEnCuentaExterna);

        }




        [HttpPost]
        public async Task<IActionResult> BorrarEmpleado(string id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsarioId();


            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);


            //Si es Null significa que estamos logeados con cuenta local y no externa

            if (AspUser == null)
            {

                var User = await appDbcontext.Usuarios.FirstOrDefaultAsync(
                  x => x.Id == usuarioId);


                if (User == null)
                {
                    notyfService.Warning("Cuenta no encontrada!");

                    return RedirectToAction("NoEncontrado", "Home");

                }


                var ExisteEmpleadoEnCuentaLocal = await ObtenerPorId(id, usuarioId);

                if (ExisteEmpleadoEnCuentaLocal == null)
                {
                    notyfService.Warning("Empleado no encontrado!");

                    return RedirectToAction("NoEncontrado", "Home");

                }

              


                User.EmpleadosDespedidos++;

                User.SalarioEmpleados -= ExisteEmpleadoEnCuentaLocal.Salario;

                appDbcontext.Empleados.Remove(ExisteEmpleadoEnCuentaLocal);
                await appDbcontext.SaveChangesAsync();
                notyfService.Custom("Empleado borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-minus");



                return RedirectToAction("Lista");
            }

            var usuario2 = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);


            if (usuario2 == null)
            {
                notyfService.Warning("Cuenta no encontrada");

                return RedirectToAction("NoEncontrado", "Home");
            }


            var ExisteEmpleadoEnCuentaExterna = await ObtenerPorId(id, AspUser.Id);



            if (ExisteEmpleadoEnCuentaExterna == null)
            {
                notyfService.Warning("Empleado no encontrado!");

                return RedirectToAction("NoEncontrado", "Home");

            }




            AspUser.EmpleadosDespedidos++;


            AspUser.SalarioEmpleados -= ExisteEmpleadoEnCuentaExterna.Salario;


            appDbcontext.Empleados.Remove(ExisteEmpleadoEnCuentaExterna);




            await appDbcontext.SaveChangesAsync();

            notyfService.Custom("Empleado borrado exitosamente", 7000, "#9cff7a", "fa-solid fa-user-minus");



            return RedirectToAction("Lista");



        }


        private async Task<IEnumerable<SelectListItem>> ObtenerDepartamentos(string usuarioId)
        {



            // Obtener el ID del usuario local o externo
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);

            if (AspUser == null)
            {
                var departamentos1 = await appDbcontext.Departamentos.Where(
     x => x.UsuarioId == usuarioId).ToListAsync();


                // Convertir los departamentos a SelectListItem para ser usados en un dropdown o similar
                return departamentos1.Select(x => new SelectListItem(
                    x.Nombre, x.Id.ToString()));
            }

            // Obtener los departamentos para el usuario
            var departamentos = await appDbcontext.Departamentos
                .Where(x => x.UsuarioId == AspUser.Id)
                .ToListAsync();

            // Convertir los departamentos a SelectListItem para ser usados en un dropdown o similar
            return departamentos.Select(x => new SelectListItem(
                x.Nombre, x.Id.ToString()));
        }




        private async Task<IEnumerable<SelectListItem>> ObtenerPosiciones(string usuarioId)
        {
            // Obtener el ID del usuario local o externo
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);

            if (AspUser == null)
            {
                var posiciones1 = await appDbcontext.Posiciones.Where(
     x => x.UsuarioId == usuarioId).ToListAsync();


                // Convertir las posiciones a SelectListItem para ser usados en un dropdown o similar
                return posiciones1.Select(x => new SelectListItem(
                    x.Nombre, x.Id.ToString()));
            }

            // Obtener las posiciones para el usuario
            var posiciones = await appDbcontext.Posiciones
                .Where(x => x.UsuarioId == AspUser.Id)
                .ToListAsync();

            // Convertir los departamentos a SelectListItem para ser usados en un dropdown o similar
            return posiciones.Select(x => new SelectListItem(
                x.Nombre, x.Id.ToString()));
        }


        private async Task<bool> ExisteCorreo(string correo, string usuarioId)
        {


            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);

            if (AspUser == null)
            {
                return await appDbcontext.Empleados.AnyAsync(c => c.Email == correo &&
                c.UsuarioId == usuarioId);

            }
            else
            {
                return await appDbcontext.Empleados.AnyAsync(c => c.Email == correo &&
           AspUser.Id == usuarioId);
            }
        }






        //Verificando Si ya existe el correo a crear antes de postear la informacion
        //esto es para en el modelo remote



        [HttpGet]
        public async Task<IActionResult> Verificar(Empleado cliente)
        {
            cliente.UsuarioId = servicioUsuarios.ObtenerUsarioId();


            var e = new bool();

            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == cliente.UsuarioId);
            if (AspUser == null)
            {

                //Si es NULL significa que es cuentaLocal

                e = await ExisteCorreo(cliente.Email, cliente.Id);

                if (e)
                {
                    return Json($"El correo {cliente.Email} ya existe!sadfsaddsad");

                }
            }
            else
            {

                e = await ExisteCorreo(cliente.Email, AspUser.AspNetUserId);

                if (e)
                {
                    return Json($"El correo {cliente.Email} ya existe!sadfsaddsad");

                }
            }




            return Json(true);

        }


        //GUARDA ESTO EN CASO QUE LO NECESITES ES EL METODO
        //VERIFICAR ARREGLADO TAMBIEN
        //[HttpGet]
        //public async Task<IActionResult> Verificar(Empleado empleado)
        //{
        //    var usuarioId = servicioUsuarios.ObtenerUsarioId();
        //    var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);

        //    if (AspUser == null)
        //    {
        //        // Usuario local
        //        var existeCorreoDuplicado = await appDbcontext.Empleados.AnyAsync(e =>
        //            e.Email == empleado.Email && e.Id != empleado.Id && e.UsuarioId == usuarioId);
        //    }
        //    else
        //    {
        //        // Usuario externo
        //        var existeCorreoDuplicado = await appDbcontext.Empleados.AnyAsync(e =>
        //            e.Email == empleado.Email && e.Id != empleado.Id && e.UsuarioId == AspUser.Id);
        //    }






        //    return Json(true);

        //}






        //Obteniendo El Empleado por su Id y el UsuarioId
        public async Task<Empleado> ObtenerPorId(string id, string usuarioId)
        {
            return await appDbcontext.Empleados
                .FirstOrDefaultAsync(tc => tc.Id == id && tc.UsuarioId == usuarioId);
        }





        public async Task<IActionResult> DownloadExcel()
        {
            var usuarioId = servicioUsuarios.ObtenerUsarioId();



            //Obteniendo Id de cuentas externas
            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u =>
            u.AspNetUserId == usuarioId);

            //Si es null estamos autenticados con cuenta local
            if (AspUser == null)
            {
                var listaClientes = await appDbcontext.Empleados.Where(c => c.UsuarioId == usuarioId).ToListAsync();

                if (listaClientes.Count == 0)
                {
                    return RedirectToAction("Lista", "Empleados");

                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Lista de empleados");

                    //columnas

                    worksheet.Cells[1, 1].Value = "Nombre";
                    worksheet.Cells[1, 2].Value = "Email";
                    worksheet.Cells[1, 3].Value = "Fecha Contrato";
                    worksheet.Cells[1, 4].Value = "Activo";

                    //estilo
                    worksheet.Cells["A1:E1"].Style.Font.Bold = true;


                    //fila 2 se agrega todo luego se suma 1 a la fila row++ para seguir agregando todo a lo demas
                    //no le ponemos row = 1 porque en el row 1 es donde estan las columnas de los nombres de los campos
                    int row = 2;

                    foreach (var cliente in listaClientes)
                    {

                        worksheet.Cells[row, 1].Value = cliente.Nombre;
                        worksheet.Cells[row, 2].Value = cliente.Email;
                        worksheet.Cells[row, 3].Value = cliente.FechaContrato.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 4].Value = cliente.Activo ? "Sí" : "No";
                        row++;
                    }



                    worksheet.Cells["A1:E1"].AutoFitColumns();

                    using (MemoryStream stream = new())
                    {
                        package.SaveAs(stream);
                        stream.Position = 0;



                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                   "ListadeEmpleados.xlsx", true);
                    }
                }


            }

            //Si no es Null , estamos autenticados con cuenta externa

            var listaClientesExterno = await appDbcontext.Empleados.Where(c => c.UsuarioId == AspUser.Id).ToListAsync();

            if (listaClientesExterno.Count == 0)
            {
                return RedirectToAction("Lista", "Empleados");

            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Lista de empleados");

                //columnas

                worksheet.Cells[1, 1].Value = "Nombre";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Fecha Contrato";
                worksheet.Cells[1, 4].Value = "Activo";

                //estilo
                worksheet.Cells["A1:E1"].Style.Font.Bold = true;


                //fila 2 se agrega todo luego se suma 1 a la fila row++ para seguir agregando todo a lo demas
                //no le ponemos row = 1 porque en el row 1 es donde estan las columnas de los nombres de los campos
                int row = 2;

                foreach (var cliente in listaClientesExterno)
                {

                    worksheet.Cells[row, 1].Value = cliente.Nombre;
                    worksheet.Cells[row, 2].Value = cliente.Email;
                    worksheet.Cells[row, 3].Value = cliente.FechaContrato.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 4].Value = cliente.Activo ? "Sí" : "No";
                    row++;
                }



                worksheet.Cells["A1:E1"].AutoFitColumns();

                using (MemoryStream stream = new())
                {
                    package.SaveAs(stream);
                    stream.Position = 0;



                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                               "ListadeEmpleados.xlsx", true);
                }
            }


        }


        public async Task<IActionResult> DescargarPDF()
        {
            var usuarioId = servicioUsuarios.ObtenerUsarioId();

            var AspUser = await appDbcontext.Usuarios.FirstOrDefaultAsync(u => u.AspNetUserId == usuarioId);




            if (AspUser == null)
            {
                var listaClientes = await appDbcontext.Empleados.Where(c => c.UsuarioId == usuarioId).ToListAsync();
                if (listaClientes.Count == 0)
                {
                    return RedirectToAction("Lista", "Empleados");
                }

                return new ViewAsPdf("TablaEmpleadosVista", listaClientes)
                {
                    FileName = "ListaDeEmpleados.pdf"

                };

            }

            var listaClientesExterno = await appDbcontext.Empleados.Where(c => c.UsuarioId == AspUser.Id).ToListAsync();


            if (listaClientesExterno.Count == 0)
            {
                return RedirectToAction("Lista", "Empleados");
            }

            return new ViewAsPdf("TablaEmpleadosVista", listaClientesExterno)
            {
                FileName = "ListaDeEmpleados.pdf"
            };
        }





    }
}
