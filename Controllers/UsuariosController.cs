using ManejoClientes.Data;
using ManejoClientes.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ManejoClientes.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbcontext appDbcontext;
        private readonly UserManager<Usuario> userManager;
        private readonly SignInManager<Usuario> signInManager;
        private readonly IServicioUsuarios servicioUsuarios;

        public UsuariosController(AppDbcontext appDbcontext,
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IServicioUsuarios servicioUsuarios)
        {
            this.appDbcontext = appDbcontext;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.servicioUsuarios = servicioUsuarios;
        }
















        [AllowAnonymous]
        public IActionResult Registro()
        {
            return View();
        }



        [AllowAnonymous]

        [HttpPost]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {


            if (!ModelState.IsValid)
            {
                return View(modelo);
            }




            var usuario = new Usuario()
            {

                Email = modelo.Email
            };

            usuario.Id = Guid.NewGuid().ToString();

            var existeIdDuplicado = await appDbcontext.Usuarios.AnyAsync(c => c.Id == usuario.Id);
            if (existeIdDuplicado)
            {
                //creando Nuevo ID en caso de que ya exista uno
                usuario.Id = Guid.NewGuid().ToString();
            }


            var resultado = await userManager.CreateAsync(usuario, modelo.Password);

            if (resultado.Succeeded)
            {
                //isPersisten para que no se deslogee aun cuando cierre el nav
                await signInManager.SignInAsync(usuario, isPersistent: true);
                return RedirectToAction("Lista", "Empleados");

            }
            else
            {
                foreach (var item in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }


                return View(modelo);
            }

        }



        [HttpPost]
        public async Task<string> CrearUsuario(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                 RedirectToAction("Lista", "Empleados");

            }

            usuario.EmailNormalizado = usuario.Email;


            await appDbcontext.Usuarios.AddAsync(usuario);
            await appDbcontext.SaveChangesAsync();

            return usuario.Id;

        }


        //public async Task<Usuario> BuscarUsuarioPorEmail(string email)
        //{
        //    var usuarios = await appDbcontext.Usuarios.Where(c => c.EmailNormalizado 
        //    == email).ToListAsync();

        //    return usuarios.FirstOrDefault();

        //}


        [HttpGet]
        [AllowAnonymous]

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }



            var resultado = await signInManager.PasswordSignInAsync(model.Email,
                model.Password, model.Recuerdame, lockoutOnFailure: false);


            if (resultado.Succeeded)
            {

                var usuarioId = servicioUsuarios.ObtenerUsarioId();

                var usuario = await appDbcontext.Usuarios.FirstOrDefaultAsync(x =>
                    x.Id == usuarioId);

            

                return RedirectToAction("Lista", "Empleados");

            }
            else
            {
                ModelState.AddModelError(string.Empty, "Escribe la contrasena");
                return View(model);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login", "Usuarios");
        }
    }
}
